using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Helpers;

public class NotificationBuilder(IServiceCollection services)
{
    // Services is public so that generated source code for Dependency Injection registration can access it with extension method
    public IServiceCollection Services { get; } = services;
}