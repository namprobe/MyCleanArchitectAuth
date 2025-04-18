using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using AutoMapper;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterDto RegisterDto) : IRequest<Result<string>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ILoggerService logger,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            await _unitOfWork.BeginTransactionAsync();

            // Sử dụng AutoMapper để map từ RegisterDto sang ApplicationUser
            var user = _mapper.Map<ApplicationUser>(request.RegisterDto);

            //tạo user với password
            var (createResult, userId) = await _identityService.CreateUserAsync(user, request.RegisterDto.Password);
            if (!createResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogWarning("Failed to create user: {Errors}", string.Join(", ", createResult.Errors));
                return Result<string>.Failure(createResult.Errors);
            }

            //gán role mặc định
            var roleResult = await _identityService.AddToRoleAsync(userId, Roles.User);
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogWarning("Failed to add role: {Errors}", string.Join(", ", roleResult.Errors));
                return Result<string>.Failure(roleResult.Errors);
            }

            //tạo token xác thực email
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(user);
            user.VerificationToken = token;

            // lưu token vào database
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("User {Email} registered successfully", user.Email);
            return Result<string>.Success(userId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error registering user {Email}", request.RegisterDto.Email);
            throw;
        }
    }
}