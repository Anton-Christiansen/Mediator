namespace Mediator.Interfaces;

/// <summary>
/// The base request handler. Derive your own custom from this
/// </summary>
/// <typeparam name="TRequest">The request</typeparam>
public interface IRequestHandler<in TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
    

/// <summary>
/// The base request handler. Derive your own custom from this
/// </summary>
/// <typeparam name="TRequest">The request</typeparam>
/// <typeparam name="TResponse">The response</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
