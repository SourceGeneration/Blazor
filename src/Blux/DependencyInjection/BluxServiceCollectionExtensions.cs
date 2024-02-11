using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SourceGeneration.ActionDispatcher;
using SourceGeneration.States;

namespace Blux;

public static class BluxServiceCollectionExtensions
{
    public static IServiceCollection AddBlux(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.AddActionDispatcher(serviceLifetime);
        services.AddScoped<NavigateActionHandler>();
        StateRegister.Register(services);
        return services;
    }

    public static IServiceCollection AddServiceProviderComponentActivator(this IServiceCollection services)
    {
        return services.Replace(ServiceDescriptor.Transient<IComponentActivator, ServiceProviderComponentActivator>());
    }
}
