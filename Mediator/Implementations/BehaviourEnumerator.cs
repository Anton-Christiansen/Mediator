namespace Mediator.Implementations;

/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <param name="behaviours">The behaviours for the handler</param>
/// <typeparam name="TRequest">Request type</typeparam>
public class BehaviourEnumerator<TRequest>(IEnumerable<object> behaviours) : IDisposable
{
    private readonly IEnumerator<object> _behaviours = behaviours.GetEnumerator();
    private CancellationToken CancellationToken { get; set; }
    private Func<TRequest, Task> Next { get; set; } = null!;
    
    public async Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken)
    {
        Next = next;
        CancellationToken = cancellationToken;
        await EnumerateAsync(request);
    }
    
    private async Task EnumerateAsync(TRequest request)
    {
        var next = EnumerateAsync;
        if (_behaviours.MoveNext())
        {
            await ((dynamic)_behaviours.Current).ExecuteAsync(request, next, CancellationToken);
            return;
        }
        
        await Next(request);
    }

    public void Dispose() => _behaviours.Dispose();
}

/// <summary>
/// Iterate through all behaviours for the given handler type
/// </summary>
/// <param name="behaviours">The behaviours for the handler</param>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class BehaviourEnumerator<TRequest, TResponse>(IEnumerable<object> behaviours) 
    : IDisposable
{
    private readonly IEnumerator<object> _behaviours = behaviours.GetEnumerator();
    private CancellationToken CancellationToken { get; set; }
    private Func<TRequest, Task<TResponse>> Next { get; set; } = null!;
    
    public async Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        Next = next;
        CancellationToken = cancellationToken;
        return await EnumerateAsync(request);
    }
    
    private async Task<TResponse> EnumerateAsync(TRequest request)
    {
        var next = EnumerateAsync;
        if (_behaviours.MoveNext())
        {
            return await ((dynamic)_behaviours.Current).ExecuteAsync(request, next, CancellationToken);
        }
        
        return await Next(request);
    }
    
    public void Dispose() => _behaviours.Dispose();
}