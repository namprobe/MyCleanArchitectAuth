using Auth.Domain.Enums;
using Microsoft.Extensions.Hosting;

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
    
    public EmailTemplateService(IHostEnvironment env)
    {
        _templatePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Templates", "Emails");
        _templateCache = new Dictionary<EmailTemplateType, string>();
    }

    public async Task<string> GetTemplateAsync(EmailTemplateType type)
    {
        // Try get from cache first
        if (_templateCache.TryGetValue(type, out string? cachedTemplate))
            return cachedTemplate;

        var fileName = $"{type}.html";
        var filePath = Path.Combine(_templatePath, fileName);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Email template {type} not found at {filePath}");
            
        var template = await File.ReadAllTextAsync(filePath);
        
        // Cache the template
        _templateCache[type] = template;
        
        return template;
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