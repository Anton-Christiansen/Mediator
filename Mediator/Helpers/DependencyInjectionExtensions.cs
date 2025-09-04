using Mediator.Implementations;
using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediator.Helpers;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorBuilder>? configure = null)
    {
        var builder = new MediatorBuilder(services);
        configure?.Invoke(builder);
        
        services.TryAddTransient<IMediator, Implementations.Mediator>();

        
        return services;
    }
}