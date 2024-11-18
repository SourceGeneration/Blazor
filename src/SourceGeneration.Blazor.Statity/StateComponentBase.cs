using Microsoft.AspNetCore.Components;
using SourceGeneration.ActionDispatcher;
using SourceGeneration.ChangeTracking;

namespace SourceGeneration.Blazor;

public abstract class StateComponentBase : ComponentBase, IHandleEvent, IAsyncDisposable
{
    private readonly Dictionary<object, IChangeTracker> _trackers = [];

    private bool _isDisposed;

    [Inject] public IActionDispatcher Dispatcher { get; private set; } = null!;
    [Inject] private IActionSubscriber Subscriber { get; set; } = null!;

    protected void Navigate(string uri, bool? forceLoad = null, bool? replace = null) => Dispatcher.Navigate(uri, forceLoad, replace);
    protected void Navigate(string uri, Dictionary<string, object?>? queryParametes, bool? forceLoad = null, bool? replace = null) => Dispatcher.Navigate(uri, queryParametes, forceLoad, replace);

    protected IDisposable Watch<TState, TValue>(TState state, Func<TState, TValue> selector, ChangeTrackingScope scope = ChangeTrackingScope.Root) where TState : State
    {
        return Watch(state, selector, null, null, scope);
    }

    protected IDisposable Watch<TState, TValue>(TState state, Func<TState, TValue> selector, Action<TValue> subscriber, ChangeTrackingScope scope = ChangeTrackingScope.Root) where TState : State
    {
        return Watch(state, selector, null, subscriber, scope);
    }

    protected IDisposable Watch<TState, TValue>(TState state, Func<TState, TValue> selector, Func<TValue, bool>? predicate, Action<TValue>? subscriber, ChangeTrackingScope scope = ChangeTrackingScope.Root) where TState : State
    {
        if (!_trackers.TryGetValue(state, out var tracker))
        {
            tracker = state.CreateTracker(state);
            tracker.OnChange(() => InvokeAsync(StateHasChanged));
            _trackers.Add(state, tracker);
        }
        return ((IChangeTracker<TState>)tracker).Watch(selector, predicate, subscriber, scope);
    }

    protected void DispatchAction(object action, CancellationToken cancellationToken = default) => Dispatcher.Dispatch(action, cancellationToken);
    protected Task DispatchActionAsync(object action, CancellationToken cancellationToken = default) => Dispatcher.DispatchAsync(action, cancellationToken);
    protected void DispatchAction<TAction>(CancellationToken cancellationToken = default) where TAction : new() => Dispatcher.Dispatch(new TAction(), cancellationToken);
    protected Task DispatchActionAsync<TAction>(CancellationToken cancellationToken = default) where TAction : new() => Dispatcher.DispatchAsync(new TAction(), cancellationToken);

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Func<TAction, Exception?, Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, async (TAction action, Exception? ex) =>
    {
        await callback(action, ex);
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction, Exception?> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback(action, ex);
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Func<TAction, Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, async (TAction action, Exception? ex) =>
    {
        await callback(action);
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback(action);
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Func<Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, async (TAction _, Exception? ex) =>
    {
        await callback();
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) =>
    {
        callback();
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(Func<TAction, Exception?, Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.RanToCompletion, async (TAction action, Exception? ex) =>
    {
        await callback(action, ex);
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });


    protected IDisposable SubscribeAction<TAction>(Action<TAction, Exception?> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.RanToCompletion, (TAction action, Exception? ex) =>
    {
        callback(action, ex);
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(Func<TAction,Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, async (TAction action, Exception? ex) =>
    {
        await callback(action);
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(Action<TAction> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) =>
    {
        callback(action);
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(Func<Task> callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, async (TAction _, Exception? ex) =>
    {
        await callback();
        if (autoChangeState)
        {
            await InvokeAsync(StateHasChanged);
        }
    });

    protected IDisposable SubscribeAction<TAction>(Action callback, bool autoChangeState = true) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) =>
    {
        callback();
        if (autoChangeState)
        {
            InvokeAsync(StateHasChanged);
        }
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                foreach (var tracker in _trackers)
                {
                    try { tracker.Value.Dispose(); } catch { }
                }
                _trackers.Clear();
                Subscriber.Unsubscribe(this);

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

