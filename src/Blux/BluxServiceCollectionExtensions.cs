using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sg.ActionDispatcher;
using Sg.States;

namespace Sg.Blux;

public static class BluxServiceCollectionExtensions
{
    public static IServiceCollection AddBlux(this IServiceCollection services)
    {
        services.AddScoped<NavigateActionHandler>();
        services.AddActionDispatcher();
        StateRegister.Register(services);
        return services;
    }

    public static IServiceCollection AddServiceProviderComponentActivator(this IServiceCollection services)
    {
        return services.Replace(ServiceDescriptor.Transient<IComponentActivator, ServiceProviderComponentActivator>());
    }
}
