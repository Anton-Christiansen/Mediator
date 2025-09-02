using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Notifier(IServiceProvider services) : INotifier
{
    public async Task NotifyAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return;
        }
        
        var requestType = request.GetType();
        
        var genericHandlerType = typeof(INotificationHandler<>);
        var handlerType = genericHandlerType.MakeGenericType(requestType);
        var handlers = services.GetServices(handlerType).ToArray();

        
        dynamic input = request;
        foreach (dynamic? handler in handlers)
        {
            if (handler is null) continue;
            await handler.HandleAsync(input, cancellationToken);
        }
        
        // var handlers = services.GetServices<INotificationHandler<TRequest>>().ToArray();
        //await Task.WhenAll(handlers.Select(dynamic (x) => x!).HandleAsync(request, cancellationToken)));
    }
}