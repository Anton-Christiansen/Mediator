using Mediator.Implementations;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediator.Helpers;


public class MediatorBuilder
{
    private bool UsingPipelines { get; set; } = false;
    public IServiceCollection Services { get; }
    private PipelineStore PipelineStore { get; } = new();
    
    
    internal MediatorBuilder(IServiceCollection  services)
    {
        Services = services;
    }
    
    public MediatorBuilder AddNotifications(Action<NotificationBuilder>? configure = null)
    {
        Services.TryAddTransient<INotifier, Notifier>();

        if (configure is not null)
        {
            var b = new NotificationBuilder(Services);
            configure.Invoke(b);
        }
        
        return this;
    }



    private void AddToDependencyInjection()
    {
        if (UsingPipelines) return;
        Services.AddSingleton(PipelineStore);
        UsingPipelines = true;
    }

    
    
    public MediatorBuilder AddVoidPipeline(Type type, Action<VoidPipelineBuilder> configure)
    {
        if ((type == typeof(IRequestHandler<>)) is false)
        {
            var interfaces = type.GetInterfaces();
            var first = interfaces[0];
            var definition = first.GetGenericTypeDefinition();

            if ((definition == typeof(IRequestHandler<>)) is false)
            {
                throw new InvalidOperationException($"type {type} does not inherit from {typeof(IRequestHandler<>)}");
            }
        }
        
        AddToDependencyInjection();
        
        var l = new VoidPipelineBuilder();
        configure(l);
        PipelineStore.Add(type, l.Types);
        
        return this;
    }
    
    public MediatorBuilder AddReturnPipeline(Type type, Action<ReturnPipelineBuilder> configure)
    {
        if ((type == typeof(IRequestHandler<,>)) is false)
        {
            var interfaces = type.GetInterfaces();
            var first = interfaces[0];
            var definition = first.GetGenericTypeDefinition();

            if ((definition == typeof(IRequestHandler<,>)) is false)
            {
                throw new InvalidOperationException($"type {type} does not inherit from {typeof(IRequestHandler<,>)}");
            }
        }
        
        
        AddToDependencyInjection();
        
        var l = new ReturnPipelineBuilder();
        configure(l);
        PipelineStore.Add(type, l.Types);
        
        return this;
    }
    
    

    
}

public class VoidPipelineBuilder
{
    internal readonly List<Type> Types = [];
    public void AddBehaviour(Type type)
    {
        if ((type == typeof(IPipelineBehaviour<>)) is false)
        {
            var interfaces = type.GetInterfaces();
            var first = interfaces[0];
            var definition = first.GetGenericTypeDefinition();

            if ((definition == typeof(IPipelineBehaviour<>)) is false)
            {
                throw new InvalidOperationException($"type {type} does not inherit from {typeof(IPipelineBehaviour<>)}");
            }
        }
        
        
        Types.Add(type);
    }
}


public class ReturnPipelineBuilder
{
    internal readonly List<Type> Types = [];
    public void AddBehaviour(Type type)
    {
        if ((type == typeof(IPipelineBehaviour<,>)) is false)
        {
            var interfaces = type.GetInterfaces();
            var first = interfaces[0];
            var definition = first.GetGenericTypeDefinition();

            if ((definition == typeof(IPipelineBehaviour<,>)) is false)
            {
                throw new InvalidOperationException($"type {type} does not inherit from {typeof(IPipelineBehaviour<,>)}");
            }
        }
        
        Types.Add(type);
    }
    
}