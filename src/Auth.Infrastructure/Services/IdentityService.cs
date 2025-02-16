
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    public Task<Result> AddToRoleAsync(string userId, string role)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        throw new NotImplementedException();
    }

    public Task<(Result Result, string UserId)> CreateUserAsync(ApplicationUser user, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RemoveFromRoleAsync(string userId, string role)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        throw new NotImplementedException();
    }
}

