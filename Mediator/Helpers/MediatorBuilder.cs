using Mediator.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Helpers;

/// <summary>
/// Source generated code will extend this with methods for dependency injection registration
/// </summary>
public class MediatorBuilder
{
    // Services is public so that generated source code for Dependency Injection registration can access it with extension method
    public IServiceCollection Services { get; }
    private readonly MediatorConfigurations _configurations = new();

    /// <summary>
    /// This will ensure that Mediator will automatically scope every request.
    /// Useful in desktop and console applications
    /// </summary>
    public MediatorBuilder UseScopedRequest()
    {
        _configurations.UseScope = true;
        return this;
    }
    
    internal MediatorBuilder(IServiceCollection services)
    {
        Services = services;
        Services.AddSingleton(_configurations);
    }
}
