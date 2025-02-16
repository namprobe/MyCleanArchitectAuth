using AutoMapper;
using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.DTOs.User;
using Auth.Domain.Entities;

namespace Auth.Application.Common.Mappings;

public class MappingProfile : Profile
{
    //1. Cú pháp cơ bản:
    // CreateMap<Source, Destination>()  // Định nghĩa mapping từ Source -> Destination
    // .ForMember(dest => dest.TargetProperty,  // Chọn property đích
    //            opt => opt.MapFrom(src => src.SourceProperty))  // Map từ property nguồn

    //2. Các loại mapping phổ biến:
    // 1. Map trực tiếp (cùng tên property)
    // CreateMap<RegisterDto, ApplicationUser>();

    // // 2. Map với transform
    // .ForMember(dest => dest.UserName, 
    //           opt => opt.MapFrom(src => src.Email.ToLower()))

    // // 3. Bỏ qua property
    // .ForMember(dest => dest.Sessions, 
    //           opt => opt.Ignore())

    // // 4. Map với giá trị mặc định
    // .ForMember(dest => dest.CreatedAt, 
    //           opt => opt.MapFrom(src => DateTime.UtcNow))

    //Lợi ích của DTO-Entity Mapping:
    // 1. Tách biệt logic xử lý từ ViewModel và Entity
    // 2. Dễ dàng thêm/sửa/xóa trường mà không ảnh hưởng đến các phương thức khác
    // 3. Tăng tính bảo mật và tính linh hoạt
    // 4. Dễ dàng thực hiện unit test với các DTO giả

    //Chức năng chính của DTO-Entity Mapping:
    //     a. Data Transfer:
    //    - DTO: Chỉ chứa data cần thiết cho client
    //    - Entity: Chứa đầy đủ data trong database

    // b. Security:
    //    - DTO: Hide sensitive information
    //    - Entity: Protect internal data model

    // c. Validation:
    //    - DTO: Validate input data
    //    - Entity: Maintain data integrity

    // d. Versioning:
    //    - DTO: Có thể thay đổi theo API version
    //    - Entity: Stable database schema

    // e. Performance:
    //    - DTO: Optimize network traffic
    //    - Entity: Full data model
    public MappingProfile()
    {
        // Register -> ApplicationUser
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            // Bỏ qua các trường không map
            .ForMember(dest => dest.Sessions, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // ApplicationUser -> UserProfileDto
        CreateMap<ApplicationUser, UserProfileDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt));

        // LoginDto -> UserSession
        CreateMap<LoginDto, UserSession>()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.LastActivity, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsRevoked, opt => opt.MapFrom(src => false))
            // Các trường sẽ được set riêng
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.Ignore())
            .ForMember(dest => dest.UserAgent, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // TokenResponseDto không cần mapping vì nó là DTO độc lập
        // Role và UserRole không cần mapping vì chúng ta sẽ xử lý riêng trong Identity
    }
}