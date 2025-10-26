using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler (IOrderingDbContext orderingDbContext) : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    /// <summary>
    /// Handles the execution logic for creating an order command.
    /// </summary>
    /// <param name="request">The create order command containing the order details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the result of the handling operation, containing the newly created order's ID.</returns>
    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = CreateOrderCommandMapper.CreateNewOrderFromDto(request.Order);
        // TODO
        return new CreateOrderCommandResult(order.Id.Value);
    }


}