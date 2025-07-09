using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Notifier(IServiceProvider services) : INotifier
{
    public async Task NotifyAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    {
        var handlers = services.GetServices<INotificationHandler<TRequest>>().ToArray();
        await Task.WhenAll(handlers.Select(x => x.HandleAsync(request, cancellationToken)));
    }
}