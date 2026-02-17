using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TimeForge.Models.Common;

namespace TimeForge.Database.Interceptors;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(
                eventData, result, cancellationToken);
        }

        IEnumerable<EntityEntry<BaseDeletableModel<string>>> entries =
            eventData
                .Context
                .ChangeTracker
                .Entries<BaseDeletableModel<string>>()
                .Where(e => e.State == EntityState.Deleted);

        foreach (EntityEntry<BaseDeletableModel<string>> deletable in entries)
        {
            deletable.State = EntityState.Modified;
            deletable.Entity.MarkDeleted();
        }
        
        return base.SavingChangesAsync(
            eventData, result, cancellationToken);
    }
}