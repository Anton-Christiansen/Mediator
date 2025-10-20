namespace Mediator.Interfaces;

public interface INotificationHandler<in TNotification>
{
    ValueTask HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}