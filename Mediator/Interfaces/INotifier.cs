namespace Mediator.Interfaces;


public interface INotifier
{
    /// <summary>
    /// Will apply to one or more notifications handler registrated with the TRequest
    /// </summary>
    /// <param name="request">The notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <returns></returns>
    Task NotifyAsync<TNotification>(TNotification request, CancellationToken cancellationToken = default);
}