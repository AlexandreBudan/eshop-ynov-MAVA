using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasket;

/// <summary>
/// Handles the execution of the <see cref="DeleteBasketCommand"/> to delete a user's shopping basket.
/// </summary>
/// <remarks>
/// Implements the <see cref="ICommandHandler{TCommand, TResponse}"/> interface to process the command within the CQRS architecture.
/// The handler utilizes an instance of <see cref="IBasketRepository"/> to perform the necessary operations for deleting the basket.
/// </remarks>
public class DeleteBasketCommandHandler(IBasketRepository repository) : ICommandHandler<DeleteBasketCommand, DeleteBasketCommandResult>
{
    /// <summary>
    /// Processes the request to handle the deletion of a user's basket.
    /// </summary>
    /// <param name="request">The <see cref="DeleteBasketCommand"/> containing the user's basket data to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing a <see cref="DeleteBasketCommandResult"/> which indicates whether the delete operation was successful.</returns>
    public async Task<DeleteBasketCommandResult> Handle(DeleteBasketCommand request,
        CancellationToken cancellationToken)
    {
        await repository.DeleteBasketAsync(request.UserName, cancellationToken).ConfigureAwait(false);
       
       return new DeleteBasketCommandResult(true);
    }
}