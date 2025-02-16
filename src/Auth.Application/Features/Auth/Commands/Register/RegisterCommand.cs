using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterDto RegisterDto) : IRequest<Result<string>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly ILoggerService _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IEmailService emailService,
        ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        //validate trước khi bắt đầu transaction
        if (request.RegisterDto.Password != request.RegisterDto.ConfirmPassword)
        {
            return Result<string>.Failure(new[] { "Passwords do not match" });
        }

        //wrap toàn bộ business logic trong transaction
        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            //kiểm tra email đã tồn tại chưa
            if (!await _unitOfWork.Users.IsEmailUniqueAsync(request.RegisterDto.Email))
            {
                return Result<string>.Failure(new[] { "Email already exists" });
            }

            //tạo user mới
            var user = new ApplicationUser
            {
                UserName = request.RegisterDto.Email,
                Email = request.RegisterDto.Email,
                FirstName = request.RegisterDto.FirstName,
                LastName = request.RegisterDto.LastName,
                DateOfBirth = request.RegisterDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            //tạo user với password
            var (createResult, userId) = await _identityService.CreateUserAsync(user, request.RegisterDto.Password);
            if (!createResult.Succeeded)
            {
                return Result<string>.Failure(createResult.Errors);
            }

            //gán role mặc định
            var roleResult = await _identityService.AddToRoleAsync(userId, Roles.User);
            if (!roleResult.Succeeded)
            {
                return Result<string>.Failure(roleResult.Errors);
            }

            //tạo token xác thực email
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(user);
            user.VerificationToken = token;

            // lưu token và database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Gửi email xác thực
            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email");
            }

            //trả về userID
            return Result<string>.Success(userId);
        });
    }
}