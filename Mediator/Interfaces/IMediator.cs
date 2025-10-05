namespace Mediator.Interfaces;


public interface IMediator : INotifier 
{
    /// <summary>
    /// Will apply to one or more pipelines if registered
    /// and then applied to a single handler
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <returns>An awaitable task</returns>
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class;
    
    /// <summary>
    /// Will apply to one or more pipelines if registered
    /// and then applied to a single handler
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns>The response</returns>
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
    
}