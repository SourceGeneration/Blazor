using Microsoft.AspNetCore.Components;
using Sg.States;
using System.Collections.Concurrent;
using System.Reflection;

namespace Blazor.Flux;

public abstract class FluxComponent : FluxComponentBase, IHandleEvent
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _states = new();

    private static PropertyInfo[] Resovle(Type componentType)
    {
        return _states.GetOrAdd(componentType, type => type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<InjectAttribute>() != null && x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(IState<>))
                .ToArray());
    }

    private readonly List<IState> _stateInstances = [];

    protected sealed override void BindingState()
    {
        OnStateBinding();

        var properites = Resovle(GetType());
        foreach (var property in properites)
        {
            var state = (IState?)property.GetValue(this);
            if (state != null)
            {
                _stateInstances.Add(state);
                state.SubscribeBindingChanged(() => InvokeAsync(StateHasChanged));
            }
        }
    }

    protected virtual void OnStateBinding() { }

    protected sealed override void DisposeState()
    {
        foreach (var state in _stateInstances)
        {
            try { state.Dispose(); } catch { }
        }
    }
}
