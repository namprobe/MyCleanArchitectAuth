namespace Auth.Domain.Common;

public static class BaseEntityExtensions
{
    public static void InitializeBaseEntity(this BaseEntity entity, string userId)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = userId;
        entity.LastModifiedAt = DateTime.UtcNow;
        entity.LastModifiedBy = userId;
    }

    public static void UpdateBaseEntity(this BaseEntity entity, string userId)
    {
        entity.LastModifiedAt = DateTime.UtcNow;
        entity.LastModifiedBy = userId;
    }
} 