using Microsoft.AspNetCore.Components;
using SourceGeneration.ActionDispatcher;
using SourceGeneration.States;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SourceGeneration.Blux;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
public abstract class BluxComponentBase : ComponentBase, IHandleEvent, IAsyncDisposable
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _states = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _stores = new();

    private static PropertyInfo[] ResovleStateProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type componentType)
    {
        return _states.GetOrAdd(componentType,
                static ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] type) => type
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute<InjectAttribute>() != null && x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(IScopedState<>))
                    .ToArray());
    }

    private static PropertyInfo[] ResovleStoreProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type componentType)
    {
        return _stores.GetOrAdd(componentType,
                static ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] type) => type
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute<InjectAttribute>() != null && x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(State<>))
                    .ToArray());
    }

    private readonly List<IState> _stateInstances = [];
    private readonly List<IDisposable> _storeDisposables = [];
    private bool _isDisposed;
    private bool _isInitialized;

    [Inject] private IActionDispatcher Dispatcher { get; set; } = null!;
    [Inject] private IActionSubscriber Subscriber { get; set; } = null!;

    protected void Navigate(string uri, bool? forceLoad = null, bool? replace = null) => Dispatcher.Navigate(uri, forceLoad, replace);
    protected void Navigate(string uri, Dictionary<string, object?>? queryParametes, bool? forceLoad = null, bool? replace = null) => Dispatcher.Navigate(uri, queryParametes, forceLoad, replace);

    protected void DispatchAction(object action, CancellationToken cancellationToken = default) => Dispatcher.Dispatch(action, cancellationToken);
    protected Task DispatchActionAsync(object action, CancellationToken cancellationToken = default) => Dispatcher.DispatchAsync(action, cancellationToken);
    protected void DispatchAction<TAction>(CancellationToken cancellationToken = default) where TAction : new() => Dispatcher.Dispatch(new TAction(), cancellationToken);
    protected Task DispatchActionAsync<TAction>(CancellationToken cancellationToken = default) where TAction : new() => Dispatcher.DispatchAsync(new TAction(), cancellationToken);

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction, Exception?> callback) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback(action, ex);
        InvokeAsync(StateHasChanged);
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction> callback) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback(action);
        InvokeAsync(StateHasChanged);
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action callback) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback();
        InvokeAsync(StateHasChanged);
    });

    protected IDisposable SubscribeAction<TAction>(Action<TAction, Exception?> callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.RanToCompletion, (TAction action, Exception? ex) =>
    {
        callback(action, ex);
        InvokeAsync(StateHasChanged);
    });

    protected IDisposable SubscribeAction<TAction>(Action<TAction> callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) =>
    {
        callback(action);
        InvokeAsync(StateHasChanged);
    });

    protected IDisposable SubscribeAction<TAction>(Action callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) =>
    {
        callback();
        InvokeAsync(StateHasChanged);
    });

    public async ValueTask DisposeAsync()
    {
        var task = OnDisposeAsync();
        Dispose(disposing: true);

        try
        {
            await task;
        }
        catch { }

        GC.SuppressFinalize(this);
    }


    public sealed override async Task SetParametersAsync(ParameterView parameters)
    {
        SetParametersValue(parameters);

        await base.SetParametersAsync(ParameterView.Empty);

        if (!_isInitialized)
        {
            _isInitialized = true;

            var stateProperites = ResovleStateProperties(GetType());
            foreach (var property in stateProperites)
            {
                var state = (IState?)property.GetValue(this);
                if (state != null)
                {
                    _stateInstances.Add(state);
                    state.SubscribeBindingChanged(() => InvokeAsync(StateHasChanged));
                }
            }

            var storeProperites = ResovleStoreProperties(GetType());
            foreach (var property in storeProperites)
            {
                var store = (IState?)property.GetValue(this);
                if (store != null)
                {
                    _storeDisposables.Add(store.SubscribeBindingChanged(() => InvokeAsync(StateHasChanged)));
                }
            }
        }
    }

    protected virtual void DisposeState()
    {
        foreach (var state in _stateInstances)
        {
            try { state.Dispose(); } catch { }
        }
        foreach (var dispose in _storeDisposables)
        {
            try { dispose.Dispose(); } catch { }
        }
    }

    protected virtual void SetParametersValue(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Subscriber.Unsubscribe(this);
                DisposeState();
                OnDispose();
            }
            _isDisposed = true;
        }
    }

    protected virtual void OnDispose() { }
    protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;

    protected virtual bool ShouldRenderOnEventHandled() => true;

    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        ShouldRender();
        if (ShouldRenderOnEventHandled())
        {
            return this.HandleEventAsync(callback, arg);
        }

        return callback.InvokeAsync(arg);
    }

    private Task HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        var task = callback.InvokeAsync(arg);
        var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
            task.Status != TaskStatus.Canceled;

        StateHasChanged();

        return shouldAwaitTask ? CallStateHasChangedOnAsyncCompletion(task) : Task.CompletedTask;
    }

    private async Task CallStateHasChangedOnAsyncCompletion(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            if (task.IsCanceled)
            {
                return;
            }

            throw;
        }

        StateHasChanged();
    }
}

