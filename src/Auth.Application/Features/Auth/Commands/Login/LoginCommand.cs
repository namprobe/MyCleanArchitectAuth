using Auth.Application.Common.DTOs.Auth;
using MediatR;
using Auth.Application.Common.Models;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Auth.Domain.Common;
using System.Linq;
using AutoMapper;

namespace Auth.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginDto LoginDto) : IRequest<Result<TokenResponseDto>>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IMapper _mapper;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IIdentityService identityService,
            ILogger<LoginCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _identityService = identityService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                // Kiểm tra user và password
                var user = await _unitOfWork.Users.GetUserByEmailAsync(request.LoginDto.Email);
                if (user == null || !user.CanLogin) 
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });

                var isPasswordValid = await _identityService.CheckPasswordAsync(user, request.LoginDto.Password);
                if (!isPasswordValid)
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });

                // Lấy tất cả session active (không bao gồm session đã revoke và hết hạn)
                var activeSessions = (await _unitOfWork.UserSessions.GetActiveSessionsAsync(user.Id))
                    .Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                    .ToList();

                var existingSession = activeSessions
                    .FirstOrDefault(s => s.DeviceId == request.LoginDto.DeviceId);
                var activeSessionCount = activeSessions.Count;
                bool oldestSessionRevoked = false;

                // Nếu đã đạt max sessions và không có session hiện tại
                if (activeSessionCount >= user.MaxAllowedSessions && existingSession == null)
                {
                    // Lấy session cũ nhất dựa trên CreatedAt thay vì LastActivity
                    var oldestSession = activeSessions
                        .OrderBy(s => s.CreatedAt)
                        .First();

                    oldestSession.IsRevoked = true;
                    oldestSession.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(oldestSession);
                    oldestSessionRevoked = true;
                    
                    _logger.LogInformation("Auto-revoked oldest session for user {UserId} on device {DeviceId}", 
                        user.Id, oldestSession.DeviceId);
                }

                // Revoke session hiện tại nếu có
                if (existingSession != null)
                {
                    existingSession.IsRevoked = true;
                    existingSession.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(existingSession);
                }

                // Tạo tokens mới
                var (accessToken, accessTokenExpiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
                var (refreshToken, refreshTokenExpiresAt) = await _tokenService.GenerateRefreshTokenAsync();

                var tokenResponse = new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenExpiresAt,
                    IsEmailVerified = user.EmailConfirmed,
                    ActiveSessions = activeSessionCount + 1, // +1 cho session mới sẽ được tạo
                    OldestSessionRevoked = oldestSessionRevoked,
                    MaxAllowedSessions = user.MaxAllowedSessions
                };

                // Tạo session mới sử dụng AutoMapper
                var session = _mapper.Map<UserSession>(request.LoginDto);
                
                // Cập nhật các giá trị bổ sung
                session.UserId = user.Id;
                session.RefreshToken = refreshToken;
                session.IpAddress = request.LoginDto.IpAddress ?? "client-ip";
                session.UserAgent = request.LoginDto.UserAgent ?? "user-agent";
                session.ExpiresAt = refreshTokenExpiresAt;
                
                session.InitializeBaseEntity(user.Id);
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




