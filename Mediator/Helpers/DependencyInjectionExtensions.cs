using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediator.Helpers;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Integrate mediator with dependency injection
    /// Generated source code should populate MediatorBuilder with
    /// extension methods for auto registraton 
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configure">Action to register handles, notifications and pipelines</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorBuilder>? configure = null)
    {
        var builder = new MediatorBuilder(services);
        configure?.Invoke(builder);
        
        services.TryAddTransient<IMediator, Implementations.Mediator>();
        services.TryAddTransient<INotifier, Implementations.Mediator>();
        
        return services;
    }
}