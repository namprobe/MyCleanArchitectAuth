using FluentValidation;

namespace Auth.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
            
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("Device ID is required");
    }
} 