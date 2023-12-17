using Microsoft.AspNetCore.Components;
using Sg.ActionDispatcher;

namespace Sg.Blux;

public abstract class BluxComponentBase : ComponentBase, IHandleEvent, IAsyncDisposable
{
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

    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction, Exception?> callback) where TAction : notnull => Subscriber.Subscribe(this, status, callback);
    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action<TAction> callback) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) => callback(action));
    protected IDisposable SubscribeAction<TAction>(ActionDispatchStatus status, Action callback) where TAction : notnull => Subscriber.Subscribe(this, status, (TAction action, Exception? ex) => callback());
    protected IDisposable SubscribeAction<TAction>(Action<TAction, Exception> callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.RanToCompletion, callback!);
    protected IDisposable SubscribeAction<TAction>(Action<TAction> callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) => callback(action));
    protected IDisposable SubscribeAction<TAction>(Action callback) where TAction : notnull => Subscriber.Subscribe(this, ActionDispatchStatus.Successed, (TAction action, Exception? ex) => callback());

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

            BindingState();
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
                // TODO: 释放托管状态(托管对象)
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _isDisposed = true;
        }
    }

    protected virtual void DisposeState() { }
    protected virtual void BindingState() { }
    protected virtual void OnDispose() { }
    protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;

    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg) => callback.InvokeAsync(arg);
}

