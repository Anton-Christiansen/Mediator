namespace Mediator.Interfaces;

/// <summary>
/// Do not directly implement IBehaviourHandler[TRequest]
/// Instead use IPipelineBehaviour[THandler, TRequest]
/// </summary>
public interface IBehaviourHandler<TRequest>
{
    ValueTask ExecuteAsync(TRequest request, Func<TRequest, ValueTask> next, CancellationToken cancellationToken);
}

/// <summary>
/// Do not directly implement IBehaviourHandler[TRequest, TResponse]
/// Instead use IPipelineBehaviour[THandler, TRequest, TResponse]
/// </summary>
public interface IBehaviourHandler<TRequest, TResponse>
{
    ValueTask<TResponse> ExecuteAsync(TRequest request, Func<TRequest, ValueTask<TResponse>> next, CancellationToken cancellationToken);
}