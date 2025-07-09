using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Helpers;

public class NotificationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}