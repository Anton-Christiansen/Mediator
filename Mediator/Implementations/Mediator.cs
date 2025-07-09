using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class Mediator(IServiceProvider services, PipelineStore pipelineStore) : IMediator
{
    public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : class
    {
        IRequestHandler<TRequest> handler = services.GetRequiredService<IRequestHandler<TRequest>>();
        var handlerType = handler.GetType().GetInterfaces().First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest>)));
        
        var behaviours = pipelineStore.Resolve<TRequest>(handlerType, services);

        var pipelineType = typeof(CommandPipeline<,>);
        pipelineType = pipelineType.MakeGenericType(typeof(IRequestHandler<TRequest>), typeof(TRequest));
        
        var instance = (CommandPipeline<IRequestHandler<TRequest>, TRequest>)Activator.CreateInstance(pipelineType, handler, behaviours)! 
             ?? throw new InvalidOperationException("Failed to make command pipeline non-generic");
        
        await instance.ExecuteAsync(request, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : class 
        where TResponse : class
    {
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var handlerType = handler.GetType().GetInterfaces().First(x => x.IsAssignableTo(typeof(IRequestHandler<TRequest, TResponse>)));

        var behaviours = pipelineStore.Resolve<TRequest, TResponse>(handlerType, services);
        
        var pipelineType = typeof(QueryPipeline<,,>);
        pipelineType = pipelineType.MakeGenericType(typeof(IRequestHandler<TRequest, TResponse>), typeof(TRequest), typeof(TResponse));
        
        var instance = (QueryPipeline<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>)Activator.CreateInstance(pipelineType, handler, behaviours)! 
             ?? throw new InvalidOperationException("Failed to make query pipeline non-generic");
        
        return await instance.ExecuteAsync(request, cancellationToken);
    }
    
}