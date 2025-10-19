namespace Mediator.Interfaces;

/// <summary>
/// Do not directly implement IBehaviourHandler[TRequest]
/// Instead use IPipelineBehaviour[THandler, TRequest]
/// </summary>
public interface IBehaviourHandler<TRequest>
{
    Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken);
}

/// <summary>
/// Do not directly implement IBehaviourHandler[TRequest, TResponse]
/// Instead use IPipelineBehaviour[THandler, TRequest, TResponse]
/// </summary>
public interface IBehaviourHandler<TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken);
}