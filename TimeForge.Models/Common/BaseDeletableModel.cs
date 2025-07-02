namespace TimeForge.Models.Common;

public class BaseDeletableModel<TId> : BaseModel<TId>
{
    public bool IsDeleted { get; set; }

    public DateTime DeletedAt { get; set; }

    public DateTime LastModified { get; set; }

    public DateTime CreatedAt { get; set; }
}