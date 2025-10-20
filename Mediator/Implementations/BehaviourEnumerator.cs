using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <typeparam name="THandler">Handler</typeparam>
/// <param name="behaviours">The behaviours for the handler</param>
/// <typeparam name="TRequest">Request type</typeparam>
public sealed class BehaviourEnumerator<TRequest>([FromKeyedServices(nameof(TRequest))]IEnumerable<IBehaviourHandler<TRequest>> behaviours, IRequestHandler<TRequest> handler) : IDisposable
{
    private readonly IEnumerator<IBehaviourHandler<TRequest>> _behaviours = behaviours.GetEnumerator();
    private CancellationToken CancellationToken { get; set; }
    
    public async Task ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        await EnumerateAsync(request);
    }
    
    private async Task EnumerateAsync(TRequest request)
    {
        if (_behaviours.MoveNext())
        {
            await _behaviours.Current.ExecuteAsync(request, EnumerateAsync, CancellationToken);
            return;
        }
        
        await handler.HandleAsync(request, CancellationToken);
    }

    public void Dispose() => _behaviours.Dispose();
}


/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <param name="behaviours">The behaviours for the handler</param>
/// <typeparam name="THandler">Handler</typeparam>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public sealed class BehaviourEnumerator<TRequest, TResponse>([FromKeyedServices(nameof(TRequest))]IEnumerable<IBehaviourHandler<TRequest, TResponse>> behaviours, IRequestHandler<TRequest, TResponse> handler) : IDisposable
{
    private readonly IEnumerator<IBehaviourHandler<TRequest, TResponse>> _behaviours = behaviours.GetEnumerator();
    private CancellationToken CancellationToken { get; set; }
    
    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        return await EnumerateAsync(request);
    }
    
    private async Task<TResponse> EnumerateAsync(TRequest request)
    {
        if (_behaviours.MoveNext())
        {
            return await _behaviours.Current.ExecuteAsync(request, EnumerateAsync, CancellationToken);
        }
        
        return await handler.HandleAsync(request, CancellationToken);
    }
    
    public void Dispose() => _behaviours.Dispose();
}