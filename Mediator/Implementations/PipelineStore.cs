using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class PipelineStore
{
    Dictionary<Type, List<Type>> Pipelines { get; } = new();
    
    
    public void Add(Type type, List<Type> behaviours)
    {
        Pipelines.Add(type, behaviours);
    }
    
    public List<IPipelineBehaviour<TRequest>> Resolve<TRequest>(Type type, IServiceProvider services)
    {
        var genericType = type.GetGenericTypeDefinition(); // typeof(ICommandHandler<>)
        var behaviourTypes = Pipelines[genericType]; // Logging-, transactional-behaviour

        List<IPipelineBehaviour<TRequest>> behaviours = [];
        
        
        foreach (var behaviourType in behaviourTypes)
        {
            var b = behaviourType.MakeGenericType(typeof(TRequest));
            // Logger behaviour
            var constructors = b.GetConstructors();
            bool succeeded = false;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters(); //  ILogger<Loggingbehaviour>
                List<object> args = [];
                bool failed = false;
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;

                    var service = services.GetService(parameterType);
                    if (service == null)
                    {
                        failed = true;
                        break;
                    }
                    
                    args.Add(service);
                }

                if (failed) continue;
                
                // construct the behaviour here
                var output = constructor.Invoke(args.ToArray());
                behaviours.Add((IPipelineBehaviour<TRequest>)output);
                succeeded = true;
                break;
            }

            if (succeeded == false)
            {
                throw new InvalidOperationException($"Failed to construct behaviour {b}");
            }
        }

        return behaviours;
    }
    
    
    public List<IPipelineBehaviour<TRequest, TResponse>> Resolve<TRequest, TResponse>(Type type, IServiceProvider services)
    {
        var genericType = type.GetGenericTypeDefinition(); // typeof(ICommandHandler<>)
        var behaviourTypes = Pipelines[genericType]; // Logging-, transactional-behaviour

        List<IPipelineBehaviour<TRequest, TResponse>> behaviours = [];
        
        
        foreach (var behaviourType in behaviourTypes)
        {
            var b = behaviourType.MakeGenericType(typeof(TRequest), typeof(TResponse));
            // Logger behaviour
            var constructors = b.GetConstructors();
            bool succeeded = false;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters(); //  ILogger<Loggingbehaviour>
                List<object> args = [];
                bool failed = false;
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;

                    var service = services.GetService(parameterType);
                    if (service == null)
                    {
                        failed = true;
                        break;
                    }
                    
                    args.Add(service);
                }

                if (failed) continue;
                
                // construct the behaviour here
                var output = constructor.Invoke(args.ToArray());
                behaviours.Add((IPipelineBehaviour<TRequest, TResponse>)output);
                succeeded = true;
                break;
            }

            if (succeeded == false)
            {
                throw new InvalidOperationException($"Failed to construct behaviour {b}");
            }
        }

        return behaviours;
    }
    
    
}