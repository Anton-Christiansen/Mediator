namespace Mediator.Interfaces;

public interface INotificationHandler<in TNotification>
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}