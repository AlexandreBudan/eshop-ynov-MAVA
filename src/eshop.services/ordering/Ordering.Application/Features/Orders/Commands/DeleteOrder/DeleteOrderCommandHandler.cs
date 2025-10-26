using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<DeleteOrderCommand, DeleteOrderCommandResult>
{
    /// <summary>
    /// Handles the operation for deleting an order based on the provided command.
    /// </summary>
    /// <param name="request">The command containing the order identifier to delete.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DeleteOrderCommandResult"/> containing the result of the delete operation.</returns>
    /// <exception cref="OrderNotFoundException">
    /// Thrown when an order with the specified identifier is not found.
    /// </exception>
    public async Task<DeleteOrderCommandResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
      // TODO
        
        return new DeleteOrderCommandResult(true);
    }
}