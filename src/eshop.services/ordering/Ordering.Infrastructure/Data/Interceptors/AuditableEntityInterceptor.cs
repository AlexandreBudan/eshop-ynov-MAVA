using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordering.Domain.Abstractions;

namespace Ordering.Infrastructure.Data.Interceptors;

/// <summary>
/// Represents a custom Entity Framework Core interceptor that handles the automatic tracking and updating
/// of audit metadata for entities implementing the <see cref="IEntity"/> interface. This class ensures that
/// auditing properties such as creation and modification timestamps, along with the identifiers of who performed
/// these actions, are correctly updated.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts the saving changes process in Entity Framework Core to update audit metadata on entities.
    /// This method ensures that entities implementing the <see cref="IEntity"/> interface have their
    /// properties (e.g., CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) automatically updated during save operations.
    /// </summary>
    /// <param name="eventData">
    /// The <see cref="DbContextEventData"/> containing the event data for the saving changes operation.
    /// It provides access to the associated <see cref="DbContext"/>.
    /// </param>
    /// <param name="result">
    /// The initial result of the save operation before interception logic is applied.
    /// </param>
    /// <returns>
    /// An <see cref="InterceptionResult{T}"/> that may modify or override the result of the saving changes operation.
    /// </returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts the asynchronous saving changes process in Entity Framework Core to update audit metadata on entities.
    /// This method ensures that entities implementing the <see cref="IEntity"/> interface have their
    /// properties (e.g., CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) automatically updated during asynchronous save operations.
    /// </summary>
    /// <param name="eventData">
    /// The <see cref="DbContextEventData"/> containing the event data for the saving changes operation.
    /// It provides access to the associated <see cref="DbContext"/>.
    /// </param>
    /// <param name="result">
    /// The initial result of the save operation before interception logic is applied.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that resolves to an <see cref="InterceptionResult{T}"/> which may modify
    /// or override the result of the asynchronous saving changes operation.
    /// </returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates the audit metadata for entities implementing the <see cref="IEntity"/> interface within the specified DbContext.
    /// This method ensures that properties such as CreatedAt, UpdatedAt, CreatedBy, and UpdatedBy are properly set
    /// for entities being added or modified during a DbContext save operation.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="DbContext"/> instance whose tracked entity entries are to be iterated for audit metadata updates.
    /// If the provided DbContext is null, no action is performed.
    /// </param>
    private static void UpdateEntities(DbContext? dbContext)
    {
        if(dbContext is null) return;

        foreach (var entry in dbContext.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "KAKE";
            }

            if (entry.State != EntityState.Added && entry.State != EntityState.Modified &&
                !entry.HasChangedOwnedEntities()) continue;
            
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            entry.Entity.UpdatedBy = "KAKE";
        }
    }
}

public static class Extensions
{
    /// <summary>
    /// Determines whether any owned entities associated with the specified <see cref="EntityEntry"/> have changed.
    /// This includes checking if any owned entities have been added or modified.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="EntityEntry"/> representing the tracked entity for which to check the changes in owned entities.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether any owned entities have been added or modified.
    /// </returns>
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}