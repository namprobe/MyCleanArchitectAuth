using Auth.Domain.Entities;
using Auth.Application.Common.DTOs.Auth;

namespace Auth.Application.Common.Interfaces;

public interface ITokenService
{
    // Tạo cả cặp token (dùng cho login)
    // Task<TokenResponseDto> GenerateTokensAsync(ApplicationUser user);
    
    // Tách riêng vResult<TokenResponseDto>.Failureiệc tạo từng loại token
    Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(ApplicationUser user);
    Task<(string token, DateTime expiresAt)> GenerateRefreshTokenAsync();
    
    // Validate tokens
    Task<(bool isValid, string userId)> ValidateAccessTokenAsync(string accessToken);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string token);
}

