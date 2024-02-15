using Microsoft.AspNetCore.Components;

namespace Blux;

public class ServiceProviderComponentActivator(IServiceProvider serviceProvider) : IComponentActivator
{
    public IComponent CreateInstance(Type componentType)
    {
        var instance = serviceProvider.GetService(componentType);

        if (instance == null)
        {
            instance = Activator.CreateInstance(componentType);
        }

        if (instance is not IComponent component)
        {
            throw new ArgumentException($"The type {componentType.FullName} does not implement {nameof(IComponent)}.", nameof(componentType));
        }

        return component;
    }
}