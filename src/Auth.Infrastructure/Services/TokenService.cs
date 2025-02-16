using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(JwtSettings jwtSettings, IUnitOfWork unitOfWork)
    {
        _jwtSettings = jwtSettings;
        _unitOfWork = unitOfWork;
    }

    public Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, string.Join(",", user.UserRoles.Select(x => x.Role.Name)))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return Task.FromResult((
            new JwtSecurityTokenHandler().WriteToken(token), 
            expiresAt
        ));
    }

    public Task<(string token, DateTime expiresAt)> GenerateRefreshTokenAsync()
    {
        //Tạo refresh token ngẫu nhiên an toàn hơn
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        //Lấy thời gian hết hạn từ config
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);

        return Task.FromResult((refreshToken, expiresAt));
    }

    // public Task<TokenResponseDto> GenerateTokensAsync(ApplicationUser user)
    // {
    //     throw new NotImplementedException();
    // }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        // Tìm session với refresh token đã truyền vào
        var session = await _unitOfWork.UserSessions.GetByRefreshTokenAsync(token);

        if (session == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Kiếm tra xem token đã bị revoked chưa
        if (session.IsRevoked)
        {
            throw new InvalidOperationException("Refresh token has been revoked");
        }
        
        //Revoke revoke
        session.IsRevoked = true;
        session.RevokedOn = DateTime.UtcNow;

        //Lưu thay đổi vào database, đã track instance session
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(bool isValid, string userId)> ValidateAccessTokenAsync(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = _jwtSettings.GetTokenValidationParameters();
            
            var principal = tokenHandler.ValidateToken(
                accessToken,
                validationParameters,
                out var validatedToken
            );

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return (false, string.Empty);
            }

            return (true, userId);
        }
        catch (Exception)
        {
            return (false, string.Empty);
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            // Tìm session với refresh token
            var session = await _unitOfWork.UserSessions.GetByRefreshTokenAsync(refreshToken);
            
            if (session == null)
            {
                // Token không tồn tại trong database
                return false;
            }

            if (session.IsRevoked)
            {
                // Token đã bị revoke
                return false;
            }

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                // Token đã hết hạn
                return false;
            }

            // Cập nhật LastActivity
            session.LastActivity = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

