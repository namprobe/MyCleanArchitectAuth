using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Constants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, string, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<string>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<string>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<string>>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(u => u.UserSessions)
                  .WithOne(s => s.User)
                  .HasForeignKey(s => s.UserId)
                  .IsRequired();
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            // Seed default roles
            entity.HasData(
                new Role 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    Name = Auth.Domain.Constants.Roles.Admin, 
                    NormalizedName = Auth.Domain.Constants.Roles.Admin.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Description = "Administrator role with full access"
                },
                new Role 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    Name = Auth.Domain.Constants.Roles.User, 
                    NormalizedName = Auth.Domain.Constants.Roles.User.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Description = "Standard user role"
                }
            );
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        builder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            
            entity.HasOne(us => us.User)
                .WithMany(u => u.UserSessions)
                .HasForeignKey(us => us.UserId)
                .IsRequired();

            entity.HasIndex(e => e.RefreshToken).IsUnique();
        });
    }
} 