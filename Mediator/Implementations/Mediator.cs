using Mediator.Helpers;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Mediator(IServiceProvider services, PipelineStore pipelineStore) : IMediator
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
        where TRequest : class
    {
        IRequestHandler<TRequest> handler = services.GetRequiredService<IRequestHandler<TRequest>>();
        var handlerType = handler.GetType().GetInterfaces()
            .First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest>)));

        List<IPipelineBehaviour<TRequest>> behaviours = [];
        if (handlerType.IsDerivedFromWithSteps(typeof(IRequestHandler<TRequest>), out var list))
        {
            foreach (var item in list)
            {
                behaviours.AddRange(pipelineStore.Resolve<TRequest>(item, services));
            }
        }

        behaviours.Reverse();
        
        var pipeline = new CommandPipeline<IRequestHandler<TRequest>, TRequest>(handler, behaviours);
        await pipeline.ExecuteAsync(request, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var handlerType = handler.GetType().GetInterfaces()
            .First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest, TResponse>)));

        List<IPipelineBehaviour<TRequest, TResponse>> behaviours = [];
        if (handlerType.IsDerivedFromWithSteps(typeof(IRequestHandler<TRequest, TResponse>), out var list))
        {
            foreach (var item in list)
            {
                behaviours.AddRange(pipelineStore.Resolve<TRequest, TResponse>(item, services));
            }
        }

        behaviours.Reverse();
        
        var pipeline =
            new QueryPipeline<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>(handler, behaviours);
        return await pipeline.ExecuteAsync(request, cancellationToken);

    }
}