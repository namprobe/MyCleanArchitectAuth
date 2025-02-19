using Auth.Application.Common.DTOs.Auth;
using MediatR;
using Auth.Application.Common.Models;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Auth.Domain.Common;
namespace Auth.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginDto LoginDto) : IRequest<Result<TokenResponseDto>>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IIdentityService identityService,
            ILogger<LoginCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                // Bước 1: Kiểm tra user và password (read-only)
                var user = await _unitOfWork.Users.GetUserByEmailAsync(request.LoginDto.Email);
                if (user == null || !user.CanLogin) 
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });

                // Kiểm tra xem user có role chưa
                if (user.UserRoles == null || !user.UserRoles.Any())
                {
                    _logger.LogWarning("User {Email} has no roles assigned", user.Email);
                    // Có thể thêm default role ở đây nếu cần
                }

                var isPasswordValid = await _identityService.CheckPasswordAsync(user, request.LoginDto.Password);
                if (!isPasswordValid)
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });

                // Bước 2: Xử lý session và update user
                var existingSession = await _unitOfWork.UserSessions.GetByDeviceIdAsync(user.Id, request.LoginDto.DeviceId);
                if (existingSession != null)
                {
                    existingSession.IsRevoked = true;
                    existingSession.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(existingSession);
                }

                // Tạo tokens
                var (accessToken, accessTokenExpiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
                var (refreshToken, refreshTokenExpiresAt) = await _tokenService.GenerateRefreshTokenAsync();

                var tokenResponse = new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenExpiresAt,
                    IsEmailVerified = user.EmailConfirmed
                };

                // Tạo session mới
                var session = new UserSession
                {
                    UserId = user.Id,
                    RefreshToken = refreshToken,
                    DeviceId = request.LoginDto.DeviceId,
                    DeviceName = request.LoginDto.DeviceName,
                    IpAddress = "client-ip",
                    UserAgent = "user-agent",
                    LastActivity = DateTime.UtcNow,
                    ExpiresAt = refreshTokenExpiresAt,
                    IsRevoked = false
                };

                session.InitializeBaseEntity(user.Id); // Initialize base entity fields
                await _unitOfWork.UserSessions.AddAsync(session);

                // Cập nhật user
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = session.IpAddress;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<TokenResponseDto>.Success(tokenResponse);
            });
        }
    }
}




