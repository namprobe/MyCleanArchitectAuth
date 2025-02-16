
using Auth.Application.Common.Interfaces;

namespace Auth.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendPasswordChangedEmailAsync(string email, string name)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetEmailAsync(string email, string name, string token)
    {
        throw new NotImplementedException();
    }

    public Task SendVerificationEmailAsync(string email, string name, string token)
    {
        throw new NotImplementedException();
    }

    public Task SendWelcomeEmailAsync(string email, string name)
    {
        throw new NotImplementedException();
    }
}

