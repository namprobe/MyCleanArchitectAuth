using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string DeviceId): IRequest<Result<TokenResponseDto>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Lấy session từ refresh token
            var session = await _unitOfWork.UserSessions.GetByRefreshTokenAsync(request.RefreshToken);
            if (session == null || session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
            {
                return Result<TokenResponseDto>.Failure(new[] { "Refresh token is invalid or expired" });
            }

            // Verify device id
            if (session.DeviceId != request.DeviceId)
            {
                return Result<TokenResponseDto>.Failure(new[] { "Invalid device id" });
            }

            // Lấy thông tin user - Sử dụng GetUserByIdAsync
            var user = await _unitOfWork.Users.GetUserByIdAsync(session.UserId);
            if (user == null || !user.CanLogin)
            {
                return Result<TokenResponseDto>.Failure(new[] { "User is not allowed to login" });
            }

            // Kiểm tra số session active
            var activeSessions = (await _unitOfWork.UserSessions.GetActiveSessionsAsync(user.Id))
                .Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
            var activeSessionCount = activeSessions.Count;
            bool oldestSessionRevoked = false;

            // Chỉ kiểm tra max sessions khi session hiện tại không phải là session cuối cùng
            if (activeSessionCount >= user.MaxAllowedSessions)
            {
                var oldestSession = activeSessions
                    .Where(s => s.RefreshToken != request.RefreshToken)
                    .OrderBy(s => s.CreatedAt)
                    .FirstOrDefault();

                if (oldestSession != null)
                {
                    oldestSession.IsRevoked = true;
                    oldestSession.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(oldestSession);
                    oldestSessionRevoked = true;
                }
            }

            // Chỉ tạo access token mới
            (string accessToken, DateTime accessTokenExpiresAt) = await _tokenService.GenerateAccessTokenAsync(user);

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = session.RefreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = session.ExpiresAt,
                IsEmailVerified = user.EmailConfirmed,
                ActiveSessions = activeSessionCount,
                OldestSessionRevoked = oldestSessionRevoked,
                MaxAllowedSessions = user.MaxAllowedSessions
            };

            // Cập nhật last activity
            session.LastActivity = DateTime.UtcNow;
            await _unitOfWork.UserSessions.UpdateAsync(session);

            // Cập nhật user activity
            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<TokenResponseDto>.Success(tokenResponse);
        });
    }
}
