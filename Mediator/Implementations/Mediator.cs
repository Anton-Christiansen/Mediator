using Mediator.Helpers;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal sealed class Mediator(IServiceProvider services, MediatorConfigurations configurations) : IMediator
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
    }


    public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    {
        if (configurations.UseScope)
        {
            using var scope = services.CreateScope();
            if (configurations.UsePipelines)
            {
                var enumerator = scope.ServiceProvider.GetRequiredService<BehaviourEnumerator<TRequest>>();
                await enumerator.ExecuteAsync(request, cancellationToken);
            }
            else
            {
                var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest>>();
                await handler.HandleAsync(request, cancellationToken);
            }

            return;
        }

        if (configurations.UsePipelines)
        {
            var enumerator = services.GetRequiredService<BehaviourEnumerator<TRequest>>();
            await enumerator.ExecuteAsync(request, cancellationToken);
        }
        else
        {
            var handler = services.GetRequiredService<IRequestHandler<TRequest>>();
            await handler.HandleAsync(request, cancellationToken);
        }
    }


    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
    {
        if (configurations.UseScope)
        {
            using var scope = services.CreateScope();
            if (configurations.UsePipelines)
            {
                var enumerator = scope.ServiceProvider.GetRequiredService<BehaviourEnumerator<TRequest, TResponse>>();
                return await enumerator.ExecuteAsync(request, cancellationToken);
            }

            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await handler.HandleAsync(request, cancellationToken);
        }

        if (configurations.UsePipelines)
        {
            var enumerator = services.GetRequiredService<BehaviourEnumerator<TRequest, TResponse>>();
            return await enumerator.ExecuteAsync(request, cancellationToken);
        }
        else
        {
            var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await handler.HandleAsync(request, cancellationToken);
        }
    }
}