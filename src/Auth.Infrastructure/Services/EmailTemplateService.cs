using Auth.Application.Common.Interfaces;
using Auth.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

public interface IEmailTemplateService 
{
    Task<string> GetTemplateAsync(EmailTemplateType type);
    string ReplaceParameters(string template, Dictionary<string, string> parameters);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatePath;
    private readonly Dictionary<EmailTemplateType, string> _templateCache;
    private readonly ILoggerService _logger;
    
    public EmailTemplateService(IHostEnvironment env, ILoggerService logger)
    {
        _templatePath = Path.Combine(env.ContentRootPath, "Templates", "Emails");
        _templateCache = new Dictionary<EmailTemplateType, string>();
        _logger = logger;
        
        // Đảm bảo thư mục tồn tại
        if (!Directory.Exists(_templatePath))
        {
            _logger.LogWarning("Template directory not found at {Path}, creating...", _templatePath);
            Directory.CreateDirectory(_templatePath);
        }
    }

    public async Task<string> GetTemplateAsync(EmailTemplateType type)
    {
        try 
        {
            // Try get from cache first
            if (_templateCache.TryGetValue(type, out string? cachedTemplate))
                return cachedTemplate;

            var fileName = $"{type}.html";
            var filePath = Path.Combine(_templatePath, fileName);
            
            _logger.LogInformation("Loading email template from {Path}", filePath);
            
            if (!File.Exists(filePath))
            {
                var errorMessage = $"Email template {type} not found at {filePath}";
                _logger.LogError(new FileNotFoundException(errorMessage), "Template not found for {Type}", type);
                throw new FileNotFoundException(errorMessage);
            }
                
            var template = await File.ReadAllTextAsync(filePath);
            
            // Cache the template
            _templateCache[type] = template;
            
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email template for {Type}", type);
            throw;
        }
    }

    public string ReplaceParameters(string template, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            template = template.Replace($"{{{{{param.Key}}}}}", param.Value);
        }
        return template;
    }
} 