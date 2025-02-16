using Auth.Application.Common.DTOs.Auth;
using MediatR;
using Auth.Application.Common.Models;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
namespace Auth.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginDto LoginDto) : IRequest<Result<TokenResponseDto>>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IIdentityService _identityService;

        // Constructor để inject các dependencies cần thiết
        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _identityService = identityService;
        }

        public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                // Bước 1: Kiểm tra user tồn tại và có thể đăng nhập
                var user = await _unitOfWork.Users.GetUserByEmailAsync(request.LoginDto.Email);
                if (user == null || !user.CanLogin) // CanLogin kiểm tra user active, không bị khóa, không bị xóa
                {
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });
                }

                // Bước 2: Xác thực mật khẩu
                var isPasswordValid = await _identityService.CheckPasswordAsync(user, request.LoginDto.Password);
                if (!isPasswordValid)
                {
                    return Result<TokenResponseDto>.Failure(new[] { "Invalid credentials" });
                }

                // Bước 3: Kiểm tra số lượng phiên đăng nhập
                if (user.HasReachedMaxSessions()) // Kiểm tra xem đã đạt giới hạn số phiên chưa
                {
                    return Result<TokenResponseDto>.Failure(new[] { "Maximum number of active sessions reached" });
                }

                // Tạo tokens riêng biệt
                var (accessToken, accessTokenExpiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
                var (refreshToken, refreshTokenExpiresAt) = await _tokenService.GenerateRefreshTokenAsync();

                var tokenResponse = new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenExpiresAt
                };

                // Bước 5: Vô hiệu hóa session cũ trên cùng thiết bị (nếu có)
                var existingSession = await _unitOfWork.UserSessions.GetByDeviceIdAsync(user.Id, request.LoginDto.DeviceId);
                if (existingSession != null)
                {
                    existingSession.IsRevoked = true;
                    existingSession.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(existingSession);
                }

                // Bước 6: Tạo session mới
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

                // Bước 7: Lưu session mới vào database
                await _unitOfWork.UserSessions.AddAsync(session);

                // Bước 8: Cập nhật thông tin đăng nhập của user
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = session.IpAddress;
                await _unitOfWork.Users.UpdateAsync(user);

                // Bước 9: Lưu tất cả thay đổi vào database
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<TokenResponseDto>.Success(tokenResponse);
            });
        }
    }
}




