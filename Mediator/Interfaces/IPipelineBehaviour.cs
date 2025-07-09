namespace Mediator.Interfaces;


public interface IPipelineBehaviour<TRequest>
{
    Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken);
}


public interface IPipelineBehaviour<TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken);
}
