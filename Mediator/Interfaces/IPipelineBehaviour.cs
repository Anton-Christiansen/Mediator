namespace Mediator.Interfaces;

/// <summary>
/// The THandler is for what handler type the behaviour will apply to.
/// Inheritance is applied.
/// So every request will go through IRequestHandler<> or IRequestHandler<,>
/// Derive your custom handler from IRequestHandler<> or IRequestHandler<,>
/// </summary>
/// <typeparam name="THandler">The handler type for which this behaviour will apply to</typeparam>
/// <typeparam name="TRequest">The request</typeparam>
public interface IPipelineBehaviour<in THandler, TRequest>
where THandler : IRequestHandler<TRequest>
{
    Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken);
}

/// <summary>
/// The THandler is for what handler type the behaviour will apply to.
/// Inheritance is applied.
/// So every request will go through IRequestHandler<> or IRequestHandler<,>
/// Derive your custom handler from IRequestHandler<> or IRequestHandler<,>
/// </summary>
/// <typeparam name="THandler">The handler type for which this behaviour will apply to</typeparam>
/// <typeparam name="TRequest">The request</typeparam>
/// <typeparam name="TResponse">The response</typeparam>
public interface IPipelineBehaviour<in THandler, TRequest, TResponse>
where THandler : IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken);
}
