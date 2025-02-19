namespace Auth.Application.Common.Interfaces
{
    public interface ILoggerService
    {
        void LogError(Exception ex, string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogInformation(string message, params object[] args);
    }
}
// Implement ở Infrastructure layer với Serilog/NLog
// public class LoggerService : ILoggerService
// {
//     private readonly ILogger _logger;
    
//     public void LogError(Exception ex, string message, params object[] args)
//     {
//         _logger.Error(ex, message, args);
//     }
// }