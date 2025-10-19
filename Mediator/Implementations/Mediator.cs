using Mediator.Helpers;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Mediator(IServiceProvider services, MediatorConfigurations configurations) : IMediator
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

        if (configurations.UseScope)
        {
            using var scope = services.CreateScope();
            await ResolveAndExecuteAsync(scope.ServiceProvider, request, cancellationToken);
            return;
        }
        
        await ResolveAndExecuteAsync(services, request, cancellationToken);
    }


    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        
        if (configurations.UseScope)
        {
            using var scope = services.CreateScope();
            return await ResolveAndExecuteAsync<TRequest, TResponse>(scope.ServiceProvider, request, cancellationToken);
        }
        
        return await ResolveAndExecuteAsync<TRequest, TResponse>(services, request, cancellationToken);
    }


    private static async Task ResolveAndExecuteAsync<TRequest>(IServiceProvider serviceProvider, TRequest request,
        CancellationToken cancellationToken = default)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        var handlerType = handler.GetType().GetInterfaces()
            .First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest>)));

        List<IBehaviourHandler<TRequest>> behaviours = [];
        var genericBehaviour = typeof(IPipelineBehaviour<,>);
        foreach (var bhs in 
                 from reduced in handlerType.GetInheritanceSteps(typeof(IRequestHandler<TRequest>)) 
                 select genericBehaviour.MakeGenericType(reduced, typeof(TRequest)) into concreteBehaviour 
                 select serviceProvider.GetServices(concreteBehaviour) into services 
                 select services.Select(x => (IBehaviourHandler<TRequest>)x!).ToArray())
        {
            behaviours.AddRange(bhs);
        }

        var enumerator = new BehaviourEnumerator<IRequestHandler<TRequest>, TRequest>(behaviours, handler);
        await enumerator.ExecuteAsync(request, cancellationToken);
    }
    
    
    
    private static async Task<TResponse> ResolveAndExecuteAsync<TRequest, TResponse>(IServiceProvider serviceProvider, TRequest request,
        CancellationToken cancellationToken = default)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var handlerType = handler.GetType().GetInterfaces()
            .First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest, TResponse>)));
        
        List<IBehaviourHandler<TRequest, TResponse>> behaviours = [];
        var genericBehaviour = typeof(IPipelineBehaviour<,,>);
        foreach (var bhs in 
                 from reduced in handlerType.GetInheritanceSteps(typeof(IRequestHandler<TRequest, TResponse>)) 
                 select genericBehaviour.MakeGenericType(reduced, typeof(TRequest), typeof(TResponse)) into concreteBehaviour 
                 select serviceProvider.GetServices(concreteBehaviour) into services 
                 select services.Select(x => (IBehaviourHandler<TRequest, TResponse>)x!).ToArray())
        {
            behaviours.AddRange(bhs);
        }
        
        var enumerator = new BehaviourEnumerator<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>(behaviours, handler);
        return await enumerator.ExecuteAsync(request, cancellationToken);
    }
}


