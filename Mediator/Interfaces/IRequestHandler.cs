namespace Mediator.Interfaces;



public interface IRequestHandler<in TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}


    
public interface IRequestHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
