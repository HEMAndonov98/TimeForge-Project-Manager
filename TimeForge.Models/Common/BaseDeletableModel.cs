using Microsoft.EntityFrameworkCore;

namespace TimeForge.Models.Common;

[Index(nameof(IsDeleted))]
public abstract class BaseDeletableModel<TId> : BaseModel<TId>
{
    public BaseDeletableModel()
    {
        this.CreatedAt = DateTime.UtcNow;
        this.LastModified = DateTime.UtcNow;
    }
    public bool IsDeleted { get; private set; } = false;

    public DateTime DeletedAt { get; private set; }

    public DateTime LastModified { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void MarkDeleted()
    {
        this.IsDeleted = true;
        this.DeletedAt = DateTime.UtcNow;
        this.MarkModified();
    }

    public void MarkModified()
    {
        this.LastModified = DateTime.UtcNow;
    }

}