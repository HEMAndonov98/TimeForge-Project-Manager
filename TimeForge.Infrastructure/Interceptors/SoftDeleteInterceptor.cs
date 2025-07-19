using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TimeForge.Models.Common;

namespace TimeForge.Infrastructure.Interceptors;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null)
        {
            return base.SavedChangesAsync(
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
            deletable.Entity.IsDeleted = true;
            deletable.Entity.DeletedAt = DateTime.UtcNow;
        }
        
        return base.SavedChangesAsync(
            eventData, result, cancellationToken);
    }
}