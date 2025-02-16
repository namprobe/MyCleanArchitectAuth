
using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.Services;

public class TokenService : ITokenService
{
    public Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public Task<(string token, DateTime expiresAt)> GenerateRefreshTokenAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TokenResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public Task RevokeRefreshTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<(bool isValid, string userId)> ValidateAccessTokenAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}

