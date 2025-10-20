using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public sealed class BehaviourEnumerator<TRequest>(IEnumerable<IBehaviourHandler<TRequest>> behaviours, IRequestHandler<TRequest> handler) : IBehaviourEnumerator<TRequest>
{
    private IEnumerator<IBehaviourHandler<TRequest>> _behaviours = null!;
    private CancellationToken CancellationToken { get; set; }
    public async ValueTask ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        _behaviours = behaviours.GetEnumerator();
        CancellationToken = cancellationToken;
        await EnumerateAsync(request);
    }
    
    
    private async ValueTask EnumerateAsync(TRequest request)
    {
        if (_behaviours.MoveNext())
        {
            await _behaviours.Current.ExecuteAsync(request, EnumerateAsync, CancellationToken);
            return;
        }
        
        _behaviours.Dispose();
        
        await handler.HandleAsync(request, CancellationToken);
    }
}


/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public sealed class BehaviourEnumerator<TRequest, TResponse>(IEnumerable<IBehaviourHandler<TRequest, TResponse>> behaviours, IRequestHandler<TRequest, TResponse> handler) : IBehaviourEnumerator<TRequest, TResponse>
{
    private IEnumerator<IBehaviourHandler<TRequest, TResponse>> _behaviours = null!; // behaviours.GetEnumerator();
    private CancellationToken CancellationToken { get; set; }
    
    public async ValueTask<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        _behaviours = behaviours.GetEnumerator();
        CancellationToken = cancellationToken;
        return await EnumerateAsync(request);
    }
    
    private async ValueTask<TResponse> EnumerateAsync(TRequest request)
    {
        if (_behaviours.MoveNext())
        {
            return await _behaviours.Current.ExecuteAsync(request, EnumerateAsync, CancellationToken);
        }

        _behaviours.Dispose();
        
        return await handler.HandleAsync(request, CancellationToken);
    }
}