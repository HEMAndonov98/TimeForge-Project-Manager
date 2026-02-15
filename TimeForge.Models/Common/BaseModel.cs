using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Constants;

namespace TimeForge.Models.Common;

public abstract class BaseModel<TId>
{
    public BaseModel()
    {
        if (_idGenerator != null)
            this.Id = _idGenerator();
    }

    private static readonly Func<TId> _idGenerator = GenerateId();

    [Key]
    public TId Id { get; protected set; } = default!;

    private static Func<TId> GenerateId()
    {
        var type = typeof(TId);

        if (type == typeof(Guid))
            return () => (TId)(object)Guid.NewGuid();

        if (type == typeof(string))
            return () => (TId)(object)Guid.NewGuid().ToString("N");

        if (type == typeof(int))
            return null!; // DB handles identity

        throw new NotSupportedException(
            $"Unsupported id type {type.Name}");
    }
}