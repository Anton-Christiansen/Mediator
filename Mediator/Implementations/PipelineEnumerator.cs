using Mediator.Interfaces;

namespace Mediator.Implementations;

/// <summary>
/// Will chain iterate all behaviours for handler type down to the base request type (IRequestHandler<>)
/// </summary>
/// <param name="behaviours">List of behaviour enumerators</param>
/// <param name="handler">Handler</param>
/// <typeparam name="THandler">Handler type</typeparam>
/// <typeparam name="TRequest">Request type</typeparam>
public class PipelineEnumerator<THandler, TRequest>(IEnumerable<BehaviourEnumerator<TRequest>> behaviours, THandler handler)
where THandler : IRequestHandler<TRequest>
{
    private readonly IEnumerator<BehaviourEnumerator<TRequest>> _behaviours = behaviours.GetEnumerator();
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
            var next = EnumerateAsync;
            await _behaviours.Current.ExecuteAsync(request, next, CancellationToken);
            return;
        }
        
        await handler.HandleAsync(request, CancellationToken);
    }
}


/// <summary>
/// Will chain iterate all behaviours for handler type down to the base request type (IRequestHandler<,>)
/// </summary>
/// <param name="behaviours">List of behaviour enumerators</param>
/// <param name="handler">Handler</param>
/// <typeparam name="THandler">Handler type</typeparam>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PipelineEnumerator<THandler, TRequest, TResponse>(IEnumerable<BehaviourEnumerator<TRequest, TResponse>> behaviours, THandler handler)
    where THandler : IRequestHandler<TRequest, TResponse>
{
    private readonly IEnumerator<BehaviourEnumerator<TRequest, TResponse>> _behaviours = behaviours.GetEnumerator();
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
            var next = EnumerateAsync;
            return await _behaviours.Current.ExecuteAsync(request, next, CancellationToken);
        }
        
        return await handler.HandleAsync(request, CancellationToken);
    }
}



