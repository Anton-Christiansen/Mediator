using Mediator.Helpers;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Mediator(IServiceProvider services) : IMediator
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
        
        var genericHandlerType = typeof(IPipelineBehaviour<,>);
        List<BehaviourEnumerator<TRequest>> behaviours = [];
        foreach (var step in handlerType.GetInheritanceSteps(typeof(IRequestHandler<TRequest>)))
        {
            var concreteHandlerType = genericHandlerType.MakeGenericType(step, typeof(TRequest));
            var bhs = services.GetServices(concreteHandlerType);
            var enumerator = new BehaviourEnumerator<TRequest>(bhs!);
            behaviours.Add(enumerator);
        }
        
        var pipeline = new PipelineEnumerator<IRequestHandler<TRequest>, TRequest>(behaviours, handler);
        
        
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


        var genericHandlerType = typeof(IPipelineBehaviour<,,>);
        List<BehaviourEnumerator<TRequest, TResponse>> behaviours = [];
        foreach (var step in handlerType.GetInheritanceSteps(typeof(IRequestHandler<TRequest, TResponse>)))
        {
            var concreteHandlerType = genericHandlerType.MakeGenericType(step, typeof(TRequest), typeof(TResponse));
            var bhs = services.GetServices(concreteHandlerType);
            var enumerator = new BehaviourEnumerator<TRequest, TResponse>(bhs!);
            behaviours.Add(enumerator);
        }
        
        var pipeline = new PipelineEnumerator<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>(behaviours, handler);
        
        return await pipeline.ExecuteAsync(request, cancellationToken);
    }
}


