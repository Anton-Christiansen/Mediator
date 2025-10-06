# Mediator

## About
This mediators purpose is to help you organize your code, and reduce some boilerplate code associated with mediator and dependency injection.

## Nuget
Install the package `Anton_Christiansen.Mediator`

## What's different about this mediator
While similar to other Mediators, this one stand out in 2 way's: The handler and the pipeline behaviour.

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

### IMediator and INotifier
Use the `SendAsync` method on `IMediator` interface. See Handler example below on `INotifier` example.
```c#
var mediator = app.Services.GetRequiredService<IMediator>();

var personId = Guid.NewGuid();
var personRequest = new Add.Request(personId, "Tom Bom");
await mediator.SendAsync(personRequest);

var getRequest = new FindPerson.Request(personId);
var response = await mediator.SendAsync<FindPerson.Request, FindPerson.Response>(getRequest);
Console.WriteLine("Name: " + response.Name); // Name: Tom Bom
```

### Handlers
```c#
using Mediator.Interfaces;

namespace Testproject;

public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>;
public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>;
public interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>;

public interface IPeopleCommandRepository { public Task AddAsync(Guid id, string newName); }
public interface IPeopleQueryRepository { public Task<string> GetByIdAsync(Guid id); }

public static class Add
{
    public record Request(Guid Id, string Name);
    public record Notification(Guid Id, string Name);

    public class Handler(IPeopleCommandRepository repository, INotifier notifier) : ICommandHandler<Request>
    {
        public async Task HandleAsync(Request request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await repository.AddAsync(request.Id, request.Name);
            await notifier.NotifyAsync(new Notification(request.Id, request.Name), cancellationToken);
        }
    }
}

public static class FindPerson
{
    public record Request(Guid Id);

    public record Response(Guid Id, string Name);

    public class Handler(IPeopleQueryRepository repository) : IQueryHandler<Request, Response>
    {
        public async Task<Response> HandleAsync(Request request,
            CancellationToken cancellationToken = new CancellationToken())
            => new (request.Id, await repository.GetByIdAsync(request.Id));
    }
}
```

#### Notification handlers

```c#
public class GreetNewPerson : INotificationHandler<Add.Notification>
{
    public Task HandleAsync(Add.Notification notification, CancellationToken cancellationToken = new CancellationToken())
    {
        Console.WriteLine("Greetings " + notification.Name);
        return Task.CompletedTask;
    }
}

public class HighFiveNewPerson : INotificationHandler<Add.Notification>
{
    public Task HandleAsync(Add.Notification notification, CancellationToken cancellationToken = new CancellationToken())
    {
        Console.WriteLine("High five " + notification.Name);
        return Task.CompletedTask;
    }
}
```

### Pipeline behaviour
Here is an example to pipeline behaviour log all requests and responses. 
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

### Source generated code
Here is the generated code from the examples above:

`MediatorHandlersDependencyInjection.g.cs`
```c#
// <auto-generated/>

using System;
using Mediator.Interfaces;
using Mediator.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testproject;

namespace Mediator.DependencyInjection;

internal static class MediatorHandlerDependencyInjectionExtension
{
  internal static MediatorBuilder AddHandlers(this MediatorBuilder builder)
  {
      builder.Services.TryAddTransient<IRequestHandler<Testproject.Add.Request>, Testproject.Add.Handler>();

		builder.Services.TryAddTransient<IRequestHandler<Testproject.FindPerson.Request, Testproject.FindPerson.Response>, Testproject.FindPerson.Handler>();
      
      return builder;
  }
}
```

`MediatorNotificationDependencyInjection.g.cs`
```c#
// <auto-generated/>

using System;
using Mediator.Interfaces;
using Mediator.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testproject;

namespace Mediator.DependencyInjection;

internal static class NotificationDependencyInjectionRegistrationExtensions
{
  internal static MediatorBuilder AddNotifications(this MediatorBuilder builder)
  {
      builder.Services.AddTransient<INotificationHandler<Testproject.Add.Notification>, Testproject.GreetNewPerson>();

		builder.Services.AddTransient<INotificationHandler<Testproject.Add.Notification>, Testproject.HighFiveNewPerson>();
      
      return builder;
  }
}
```

`MediatorBehaviourDependencyInjectionExtensions.g.cs`
```c#
// <auto-generated/>

using System;
using Mediator.Interfaces;
using Mediator.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testproject;

namespace Mediator.DependencyInjection;

internal static class BehaviourDependencyInjectionRegistrationExtensions
{
  internal static MediatorBuilder AddPipelines(this MediatorBuilder builder)
  {
      builder.Services.AddTransient<IPipelineBehaviour<Mediator.Interfaces.IRequestHandler<Testproject.Add.Request>, Testproject.Add.Request>, LoggingBehaviour<Testproject.Add.Request>>();

		builder.Services.AddTransient<IPipelineBehaviour<Mediator.Interfaces.IRequestHandler<Testproject.FindPerson.Request, Testproject.FindPerson.Response>, Testproject.FindPerson.Request, Testproject.FindPerson.Response>, LoggingBehaviour<Testproject.FindPerson.Request, Testproject.FindPerson.Response>>();
      
      return builder;
  }
}
```
