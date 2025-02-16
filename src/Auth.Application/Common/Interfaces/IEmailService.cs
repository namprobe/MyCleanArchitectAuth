namespace Auth.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string name, string token);
    Task SendPasswordResetEmailAsync(string email, string name, string token);
    Task SendWelcomeEmailAsync(string email, string name);
    Task SendPasswordChangedEmailAsync(string email, string name);
}