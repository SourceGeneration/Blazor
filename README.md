# Blux

[![NuGet](https://img.shields.io/nuget/vpre/SourceGeneration.Blux.svg)](https://www.nuget.org/packages/SourceGeneration.Blux)

Blux is a Blazor **flux** framework base on [`States`](https://github.com/SourceGeneration/States) and [`ActionDispather`](https://github.com/SourceGeneration/ActionDispatcher), and it supports AOT compilation

## Installing

```powershell
Install-Package SourceGeneration.Blux -Version 1.0.0-beta2.240221.1
```

```powershell
dotnet add package SourceGeneration.Blux --version 1.0.0-beta2.240221.1
```

## DependencyInjection

**WebAssembly or Hybird**
```c#
services.AddBlux(ServiceLifetime.Singleton);
```

**Server**
```c#
services.AddBlux(ServiceLifetime.Scoped);
```

## State Management

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
```c#
@inherits BluxComponentBase
@inject IScopedState<MyState> State

<p>Current count: @currentCount</p>

<button @onclick="IncrementCount">Click me</button>

@code{
    private int currentCount;

    protected void OnStateBinding()
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

**Component**
```c#
@inherits BluxComponentBase
@inject IScopedState<MyState> State

<p>Current count: @currentCount</p>

<button @onclick="IncrementCount">Click me</button>

@code{
    private int currentCount;

    protected void OnStateBinding()
    {
        //Binding state.Count property to local field
        State.Bind(x => x.Count, x => currentCount = x);
    }

    private void IncrementCount()
    {
        DispatchAction(new Increment());
    }
}
```

## Action Subscriber


```c#
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

## Comparison with Fluxor

- Blux is based on Source Generator, it's faster and `AOTable`
- Blux is not an implementation of Redux, it's simpler and easier to use.
- Blux supports `Reactive(Rx)`, it's more flexible.
- Blux supports ChangeScope, it will be minimize calls to StateHasChanged as much as possible