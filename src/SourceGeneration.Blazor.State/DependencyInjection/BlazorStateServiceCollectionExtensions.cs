using Microsoft.Extensions.DependencyInjection;
using SourceGeneration.ActionDispatcher;

namespace SourceGeneration.Blazor;

public static class BlazorStateServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorState(this IServiceCollection services, ServiceLifetime dispatcherLifetime = ServiceLifetime.Scoped, ServiceLifetime subscriberLifetime = ServiceLifetime.Scoped)
    {
        services.AddActionDispatcher(dispatcherLifetime, subscriberLifetime);
        services.AddScoped<NavigateActionHandler>();
        return services;
    }
}
