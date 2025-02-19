using Auth.Application.Common.Interfaces;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggerService _logger;

    public RegisterCommandValidator(IUnitOfWork unitOfWork, ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        RuleFor(x => x.RegisterDto.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email {PropertyValue} already exists");

        RuleFor(x => x.RegisterDto.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.RegisterDto.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.RegisterDto.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.RegisterDto.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.RegisterDto.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.RegisterDto.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAValidAge).WithMessage("Age must be between 18 and 100 years");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(email)) return false;
            var isUnique = await _unitOfWork.Users.IsEmailUniqueAsync(email);
            _logger.LogInformation("Email uniqueness check for {Email}: {IsUnique}", email, isUnique);
            return isUnique;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email uniqueness for {Email}", email);
            return false;
        }
    }

    private bool BeAValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return false;

        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;

        return age >= 0 && age <= 100;
    }
} 