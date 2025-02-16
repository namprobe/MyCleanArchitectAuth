
using Auth.Application.Common.Interfaces;

namespace Auth.Infrastructure.Services;

public class LoggerService : ILoggerService
{
    public void LogError(Exception ex, string message, params object[] args)
    {
        throw new NotImplementedException();
    }

    public void LogWarning(string message, params object[] args)
    {
        throw new NotImplementedException();
    }
}

