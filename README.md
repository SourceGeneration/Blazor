# Blazor Statity

[![NuGet](https://img.shields.io/nuget/vpre/SourceGeneration.Blazor.Statity.svg)](https://www.nuget.org/packages/SourceGeneration.Blazor.Statity)

Blazor **flux** framework base on [`ChangeTracking`](https://github.com/SourceGeneration/ChangeTracking) and [`ActionDispather`](https://github.com/SourceGeneration/ActionDispatcher), and it supports `AOT` compilation

## Installing

This library uses C# preview features `partial property`, Before using this library, please ensure the following prerequisites are met:
- Visual Studio is version 17.11 preview 3 or higher.
- To enable C# language preview in your project, add the following to your .csproj file
```c#
<PropertyGroup>  
  <LangVersion>13</LangVersion>  
</PropertyGroup>  
```

```powershell
Install-Package SourceGeneration.Blazor.Statity -Version 1.0.0-beta3.241113.1
```

```powershell
dotnet add package SourceGeneration.Blazor.Statity --version 1.0.0-beta3.241113.1
```

## DependencyInjection

```c#
services.AddBlazorStatily();
```

## State Management

This library uses `SourceGeneration.ChangeTracking` for state management. view [document](https://github.com/SourceGeneration/ChangeTracking).

**Defining state**
```c#
[ChangeTacking]
public partial class MyState : State<MyState>
{
    public partial int Count { get; set; }
}
```
using services inject

```c#
//WebAssembly or Hybird
services.AddSingleton<MyState>();

//Server
services.AddScoped<MyState>();
```

**Component**

Inherits StateComponentBase, StateComponentBase automatically invokes StateHasChanged when after state binding has changed.

By default, which automatically invokes StateHasChanged after the component's event handlers are invoked.
In scenarios where rendering is automatically handled through state management, triggering a rerender after an event handler is invoked is often unnecessary or undesirable.
Override `ShouldRenderOnEventHandled` to control the behavior of Blazor's event handling.

```razor
@inherits StateComponentBase
@inject MyState State

<p>Current count: @State.Count</p>

<button @onclick="IncrementCount">Click me</button>

@code{
    private int currentCount;

    protected override bool ShouldRenderOnEventHandled() => false;

    protected override void OnInitialized()
    {
        //Subscribe state.Count property
        Watch(State, x => x.Count);
    }

    private void IncrementCount()
    {
        State.Count++;
        State.AcceptChanges();
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
@inherits StateComponentBase
@inject UndoState State

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
        Watch(State, x => x.List, x => UndoList = x, ChangeTrackingScope.Cascading);
    }

    private void UpdateTitle(int id)
    {
        State.List.First(x => x.Id == id).Title = "title changed";
        State.AccpetChanges();
    }
}
```

At another scenario where we delegate the rendering of the Undo object to a separate child component called UndoComponent. When a property of a specific Undo object changes, we don't want to trigger the rendering of the list component. This can be achieved using ChangeTrackingScope.Root.

```razor
@inherits StateComponentBase
@inject UndoState State

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
        Watch(State, x => x.List, x => UndoList = x, ChangeTrackingScope.Root);
    }

    private void AddUndo()
    {
        State.List.Add(new Undo());
        State.AcceptChanges();
    }
}
```

## Action Dispatcher

This library uses SourceGeneration.ActionDispatcher for event dispatch & subscribe. view [document](https://github.com/SourceGeneration/ActionDispatcher).

**Defining action**
```c#
public class Increment
{
    // Parameters
}
```

**Defining action handler**
```c#
public class DefaultActionHandler(MyState state)
{
    [ActionHandler]
    public void Handle(Increment action)
    {
        state.Count++;
        state.AcceptChanges();
    }
}
```

## Action Subscriber


```razor
@inherits StateComponentBase

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

- Based on `Source Generator`, it's faster and `AOTable`
- Not an implementation of `Redux`, it's simpler and easier to use.
- Supports `ChangeScope`, it will be minimize calls to StateHasChanged as much as possible
