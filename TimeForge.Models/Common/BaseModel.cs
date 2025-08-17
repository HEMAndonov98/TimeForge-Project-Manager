using System.ComponentModel.DataAnnotations;

using TimeForge.Common.Constants;

namespace TimeForge.Models.Common;

public abstract class BaseModel<TId>
{
    [Key]
    [MaxLength(BaseModelConstants.IdGuidMaxLength)]
    public TId Id { get; set; }
}