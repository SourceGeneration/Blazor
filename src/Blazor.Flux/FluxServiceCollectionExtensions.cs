using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sg.ActionDispatcher;
using Sg.States;

namespace Blazor.Flux;

public static class FluxServiceCollectionExtensions
{
    public static IServiceCollection AddFlux(this IServiceCollection services)
    {
        services.AddActionDispatcher();
        StateRegister.Register(services);
        return services;
    }

    public static IServiceCollection UseServiceProviderComponentActivator(this IServiceCollection services)
    {
        return services.Replace(ServiceDescriptor.Transient<IComponentActivator, ServiceProviderComponentActivator>());
    }
}
