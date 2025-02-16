using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Auth.Application.Common.Behaviors;
using Auth.Application.Common.Mappings;
using Auth.Application.Common.Interfaces;
namespace Auth.Application;

public static class DependencyInjection
{
    // 1. Extension Method cho IServiceCollection
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 2. AutoMapper Registration
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        // - Assembly.GetExecutingAssembly(): Lấy assembly hiện tại (Auth.Application)
        // - Tự động scan và đăng ký tất cả Profile trong assembly

        // 3. MediatR Registration
        services.AddMediatR(cfg =>
        {
            // Đăng ký tất cả handlers trong assembly
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Pipeline Behaviors (theo thứ tự)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            // - IPipelineBehavior<,>: Interface generic cho pipeline
            // - Behaviors sẽ được thực thi theo thứ tự đăng ký
        });

        // 4. FluentValidation Registration
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // - Tự động scan và đăng ký tất cả validators trong assembly

        // 5. Register Pipeline Behaviors với Dependency Injection
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        // - AddTransient: Service được tạo mới mỗi lần request
        // - typeof(IPipelineBehavior<,>): Interface service
        // - typeof(ValidationBehavior<,>): Implementation class

        return services;
    }
}
