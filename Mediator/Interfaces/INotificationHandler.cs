namespace Mediator.Interfaces;

public interface INotificationHandler<in TRequest>
{
    Task HandleAsync(TRequest notification, CancellationToken cancellationToken = default);
}