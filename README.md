# Blux

[![NuGet](https://img.shields.io/nuget/vpre/SourceGeneration.Blux.svg)](https://www.nuget.org/packages/SourceGeneration.Blux)

Blux is a Blazor **flux** framework base on [`States`](https://github.com/SourceGeneration/States) and [`ActionDispather`](https://github.com/SourceGeneration/ActionDispatcher), and it supports AOT compilation

## Installing

```powershell
Install-Package SourceGeneration.Blux -Version 1.0.0-beta2.240609.1
```

```powershell
dotnet add package SourceGeneration.Blux --version 1.0.0-beta2.240609.1
```

## DependencyInjection

```c#
services.AddBlux();
```

## State

Blux uses SourceGeneration.States for state management. view [document](https://github.com/SourceGeneration/States).

**Defining state**
```c#
[StateInject]
public class MyState
{
    public int Count { get; set; }
}
```
Or using services inject

```c#
services.AddState<MyState>();
```

**Component**

Inherits BluxComponentBase, BluxComponentBase automatically invokes StateHasChanged when after state binding has changed.

By default, which automatically invokes StateHasChanged after the component's event handlers are invoked.
In scenarios where rendering is automatically handled through state management, triggering a rerender after an event handler is invoked is often unnecessary or undesirable.
Override `ShouldRenderOnEventHandled` to control the behavior of Blazor's event handling.

```razor
@inherits BluxComponentBase
@inject IScopedState<MyState> State

<p>Current count: @currentCount</p>

<button @onclick="IncrementCount">Click me</button>

@code{
    private int currentCount;

    protected override bool ShouldRenderOnEventHandled() => false;

    protected override void OnInitialized()
    {
        //Binding state.Count property to local field
        State.Bind(x => x.Count, x => currentCount = x);
    }

    private void IncrementCount()
    {
        State.Update(x => x.Count++);
    }
}
```

> **Note** 
Don't directly update `State` within the `Component`.
The code provided here is just an example of State.
Instead, you should dispatch actions and update State within the action handler.

## Change Scope

Consider the following scenario where a component binds to an `UndoList`, and the `Undo` object and `List` are rendered in the same component without using child components. When we modify the `Undo` property, we need to rerender the page. This can be achieved using `·`ChangeTrackingScope.Cascading`.

You can specify the scope of the subscribed changes.

- **ChangeTrackingScope.Root** `default value`  
  The subscription only be triggered when there are changes in the properties of the object itself.
- **ChangeTrackingScope.Cascading**  
  The subscription will be triggered when there are changes in the properties of the object itself or in the properties of its property objects.
- **ChangeTrackingScope.Always**  
  The subscription will be triggered whenever the `Update` method is called, regardless of whether the value has changed or not.

```razor
@inherits BluxComponentBase
@inject IScopedState<UndoState> State

<ul>
    @foreach(var undo in UndoList)
    {
        <li>
            <h5>@undo.Title</h5>
            <p>@undo.Done</p>
            <button @onclick="@(() => UpdateTitle(undo.Id))">Update</button>
        </li>
    }
</ul>

@code{
    private ChangeTrackingList<Undo> UndoList;

    protected override bool ShouldRenderOnEventHandled() => false;

    protected override OnInitialized()
    {
        //ChangeTrackingScope.Cascading
        State.Bind(x => x.List, x => UndoList = x, ChangeTrackingScope.Cascading);
    }

    private void UpdateTitle(int id)
    {
        State.Update(state => 
        {
            state.List.First(x => x.Id == id).Title = "title changed";
        });
    }
}
```

At another scenario where we delegate the rendering of the Undo object to a separate child component called UndoComponent. When a property of a specific Undo object changes, we don't want to trigger the rendering of the list component. This can be achieved using ChangeTrackingScope.Root.

```razor
@inherits BluxComponentBase
@inject IScopedState<UndoState> State

<div>
    @foreach(var undo in UndoList)
    {
        <UndoComponent Id="@undo.Id" />
    }
</div>

<button @onclick="AddUndo">Add Undo</button>

@code{
    private ChangeTrackingList<Undo> UndoList;

    protected override bool ShouldRenderOnEventHandled() => false;

    protected override OnInitialized()
    {
        //ChangeTrackingScope.Root
        State.Bind(x => x.List, x => UndoList = x, ChangeTrackingScope.Root);
    }

    private void AddUndo()
    {
        State.Update(x => 
        {
            x.List.Add(new Undo());
        });
    }
}
```

## Reactive(Rx)

State implement `IObservable<T>`, so you can use Rx framework like `System.Reactive`,  
- States does not have a dependency on `System.Reactive`.
- Subscribe `IObservable` must manually invoke StateHasChanged

```razor
@using System.Reactive
@inherits BluxComponentBase
@inject IScopedState<UndoState> State

<div>
    @foreach(var undo in UndoList)
    {
        <UndoComponent Id="@undo.Id" />
    }
</div>

<button @onclick="AddUndo">Add Undo</button>

@code{
    private IEnumerable<Undo> UndoList;

    protected override OnInitialized()
    {
        State
            .Select(x => x.List)
            .Where(x => x.Done)
            .DistinctUntilChanged()
            .Subscribe(x => 
            {
                UndoList = x;
                InvokeAsync(StateHasChanged);
            });
    }
}
```

## Action Dispatcher

Blux uses SourceGeneration.ActionDispatcher for event dispatch & subscribe. view [document](https://github.com/SourceGeneration/ActionDispatcher).

**Defining action**
```c#
public class Increment
{
    // Parameters
}
```

**Defining action handler**
```c#
public class DefaultActionHandler(State<MyState> state)
{
    [ActionHandler]
    public void Handle(Increment action)
    {
        state.Update(x => x.Count++);
    }
}
```

> **Note** 
Don't use `IScopedState` outside of the `Component`.
`IScopedState` is a transient object that requires you to manually handle its Dispose. 
Instead, you should use `State<T>` objects as replacements. `State<T>` based on your dependency injection configuration, can be singleton or scoped, and you don't need to worry about whether they need to be disposed.

**Component**
```razor
@inherits BluxComponentBase
@inject IScopedState<MyState> State

<p>Current count: @currentCount</p>

<button @onclick="IncrementCount">Click me</button>

@code{
    private int currentCount;

    protected override bool ShouldRenderOnEventHandled() => false;

    protected override OnInitialized()
    {
        // Binding Count, When Count is greater than 10, push it to a local field
        State.Bind(x => x.Count, x => x > 10 x => currentCount = x);
    }

    private void IncrementCount()
    {
        DispatchAction(new Increment());
    }
}
```

## Action Subscriber


```razor
@inherits BluxComponentBase

@code{
    protected void OnInitialized()
    {
        SubscribeAction<Increment>(action => 
        {
            //Do something
        });
    }
}
```

## Features

- Blux is based on `Source Generator`, it's faster and `AOTable`
- Blux is not an implementation of `Redux`, it's simpler and easier to use.
- Blux supports `ChangeScope`, it will be minimize calls to StateHasChanged as much as possible
- Blux supports `Reactive(Rx)`, it's more flexible.
