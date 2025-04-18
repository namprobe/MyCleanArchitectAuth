using Auth.Domain.Enums;

namespace Auth.Application.Common.Interfaces;
public interface IEmailTemplateService 
{
    Task<string> GetTemplateAsync(EmailTemplateType type);
    string ReplaceParameters(string template, Dictionary<string, string> parameters);
}