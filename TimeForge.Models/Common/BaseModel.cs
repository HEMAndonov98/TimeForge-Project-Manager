using System.ComponentModel.DataAnnotations;

namespace TimeForge.Models.Common;

public abstract class BaseModel<TId>
{
    [Key]
    public TId Id { get; set; }
}