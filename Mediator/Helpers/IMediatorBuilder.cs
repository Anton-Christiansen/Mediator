using Mediator.Implementations;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediator.Helpers;

public class MediatorBuilder
{
    // Services is public so that generated source code for Dependency Injection registration can access it with extension method
    public IServiceCollection Services { get; }
    private PipelineStore PipelineStore { get; } = new();
    private bool UsingPipelines { get; set; } = false;
    

    internal MediatorBuilder(IServiceCollection services)
    {
        Services = services;
    }

    private void RegisterPipelineStore()
    {
        if (UsingPipelines) return;

        Services.AddSingleton(PipelineStore);
        UsingPipelines = true;
    }

    public MediatorBuilder AddNotifications(Action<NotificationBuilder>? configure = null)
    {
        Services.TryAddTransient<INotifier, Implementations.Mediator>();

        if (configure is not null)
        {
            var b = new NotificationBuilder(Services);
            configure.Invoke(b);
        }

        return this;
    }

    public MediatorBuilder AddPipeline(Type type, Action<PipelineBuilder> configure)
    {
        if (type.IsDerivedFrom(typeof(IRequestHandler<>)) is false &&
            type.IsDerivedFrom(typeof(IRequestHandler<,>)) is false)
        {
                throw new InvalidOperationException($"type {type} does not inherit from {typeof(IRequestHandler<>)} or {typeof(IRequestHandler<,>)}");
        }

        RegisterPipelineStore();

        bool hasReturnType = type.IsDerivedFrom(typeof(IRequestHandler<,>));

        var l = new PipelineBuilder(hasReturnType);
        configure(l);

        PipelineStore.Add(type, l.Types);

        return this;
    }
}

public class PipelineBuilder(bool hasReturnType)
{
    private bool HasReturnType { get; set; } = hasReturnType;
    internal readonly List<Type> Types = [];

    public void AddBehaviour(Type type)
    {
        if (HasReturnType)
        {
            if (type.IsDerivedFrom(typeof(IPipelineBehaviour<,>)) is false)
            {
                throw new InvalidOperationException(
                    $"type {type} does not inherit from {typeof(IPipelineBehaviour<,>)}");
            }
        }
        else
        {
            if (type.IsDerivedFrom(typeof(IPipelineBehaviour<>)) is false)
            {
                throw new InvalidOperationException(
                    $"type {type} does not inherit from {typeof(IPipelineBehaviour<>)}");
            }
        }

        Types.Add(type);
    }
}