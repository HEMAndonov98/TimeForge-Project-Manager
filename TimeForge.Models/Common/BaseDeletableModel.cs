using Microsoft.EntityFrameworkCore;

namespace TimeForge.Models.Common;

[Index(nameof(IsDeleted))]
public abstract class BaseDeletableModel<TId> : BaseModel<TId>
{
    public bool IsDeleted { get; set; } = false;

    public DateTime DeletedAt { get; set; }

    public DateTime LastModified { get; set; }

    public DateTime CreatedAt { get; set; }
}