namespace Mediator.Interfaces;

public interface INotifier
{
    Task NotifyAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default);
}