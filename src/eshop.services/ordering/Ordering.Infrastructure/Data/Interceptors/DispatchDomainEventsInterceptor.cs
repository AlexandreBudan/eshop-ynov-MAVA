using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordering.Domain.Abstractions;

namespace Ordering.Infrastructure.Data.Interceptors;

/// <summary>
/// Intercepts the database context save process to handle dispatching of domain events for entities
/// implementing the <see cref="IAggregate"/> interface. This allows for separation of concerns
/// by ensuring domain events are executed after changes are successfully tracked in the database context.
/// </summary>
public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    /// <summary>
    /// Handles dispatching of domain events before changes are saved to the database context.
    /// </summary>
    /// <param name="eventData">Contextual information about the database event.</param>
    /// <param name="result">Interception result that can be modified during the save operation.</param>
    /// <returns>A modified or unmodified interception result for the save operation.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Handles dispatching of domain events asynchronously before changes are saved to the database context.
    /// </summary>
    /// <param name="eventData">Contextual information about the database event.</param>
    /// <param name="result">Interception result that can be modified during the save operation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, including the interception result for the save operation.</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Dispatches all domain events for the entities tracked by the given database context.
    /// </summary>
    /// <param name="context">The database context containing the tracked entities with domain events.</param>
    /// <returns>A task representing the asynchronous operation of dispatching domain events.</returns>
    private async Task DispatchDomainEventsAsync(DbContext? context)
    {
        if (context == null) return;

        var aggregates = context.ChangeTracker
            .Entries<IAggregate>()
            .Where(a => a.Entity.DomainEvents.Any())
            .Select(a => a.Entity);

        var aggregatesList = aggregates.ToList();
        
        if(aggregatesList.Count == 0) return;
        
        var domainEvents = aggregatesList
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregatesList.ToList().ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}