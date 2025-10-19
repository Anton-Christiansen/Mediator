using Mediator.Interfaces;

namespace Mediator.Implementations;

/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <typeparam name="THandler">Handler</typeparam>
/// <param name="behaviours">The behaviours for the handler</param>
/// <typeparam name="TRequest">Request type</typeparam>
internal class BehaviourEnumerator<THandler, TRequest>(IEnumerable<IBehaviourHandler<TRequest>> behaviours, THandler handler) : IDisposable
where THandler : IRequestHandler<TRequest>
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
internal class BehaviourEnumerator<THandler, TRequest, TResponse>(IEnumerable<IBehaviourHandler<TRequest, TResponse>> behaviours, THandler handler) 
    : IDisposable
    where THandler : IRequestHandler<TRequest, TResponse>
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