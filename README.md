# Mediator

## Nuget
Install the package `Anton_Christiansen.Mediator`

### Handlers
You derive your own custom handlers (for example: a `ICommandHandler` and/or a `IQueryHandler`) from the `IRequestHandler<>` or `IRequestHandler<,>` inferface.

### Pipeline behaviours
The interfaces `IPipelineBehaviour<THandler, TRequest>` and `IPipelineBehaviour<THandler, TRequest, TResponse>` takes a `THandler` with constraints of `IRequestHandler<>` or `IRequestHandler<,>`.
The behaviour will apply to `THandler` AND handlers derived from `THandler`.

## Examples

### Dependency injection
Source generator will populate the `MediatorBuilder` with the methods `AddHandlers()`, `AddNotifications()` and `AddPipelines()`
that will register the handlers and pipeline behaviours to `IServiceCollection`. It's that simple. 
```c#
builder.Services.AddMediator(mediatorBuilder =>
    mediatorBuilder
        .AddHandlers()
        .AddNotifications()
        .AddPipelines()
    );
```


### Pipeline behaviour
Here is an example of a pipeline behaviours to log all requests and responses going to and from all handlers. 
```c#
using Mediator.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Behaviours;


public class LoggingBehaviour<TRequest>(ILogger<LoggingBehaviour<TRequest>> logger) : IPipelineBehaviour<IRequestHandler<TRequest>, TRequest>
{
    public async Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken)
    {
        logger.LogInformation(request?.ToString());
        await next(request);
    }
}

public class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) : IPipelineBehaviour<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>
{
    public async Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        logger.LogInformation(request?.ToString());
        var response = await next(request);
        logger.LogInformation(response?.ToString());
        return response;
    }
}
```

Here is an example of pipeline behaviour to open a transaction on every request going to `ICommandHandler<>` or `ICommandHandler<,>` or handlers derived from either.
```c#
public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>;
public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>;

public class TransactionBehaviour<TRequest>(IUnitOfWork dbContext) : IPipelineBehaviour<ICommandHandler<TRequest>, TRequest>
{
    public async Task ExecuteAsync(TRequest request, Func<TRequest, Task> next, CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await next(request);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

public class TransactionBehaviour<TRequest, TResponse>(IUnitOfWork dbContext) : IPipelineBehaviour<ICommandHandler<TRequest, TResponse>,  TRequest, TResponse>
{
    public async Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await next(request);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```
