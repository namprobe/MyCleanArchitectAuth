using Auth.Application.Common.Models;
using Auth.Domain.Entities;

namespace Auth.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<(Result Result, string UserId)> CreateUserAsync(ApplicationUser user, string password);
    Task<Result> DeleteUserAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<Result> AddToRoleAsync(string userId, string role);
    Task<Result> RemoveFromRoleAsync(string userId, string role);
    Task<Result> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<Result> ConfirmEmailAsync(ApplicationUser user, string token);
}
