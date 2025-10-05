using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Helpers;

/// <summary>
/// Source generated code will extend this with methods for dependency injection registration
/// </summary>
public class MediatorBuilder
{
    // Services is public so that generated source code for Dependency Injection registration can access it with extension method
    public IServiceCollection Services { get; }


    internal MediatorBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
