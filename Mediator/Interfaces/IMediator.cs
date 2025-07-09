using System.ComponentModel.Design;

namespace Mediator.Interfaces;


public interface IMediator
{
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class;
    
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
}