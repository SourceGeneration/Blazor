using SourceGeneration.ActionDispatcher;
using SourceGeneration.Blux.Sample.States;
using SourceGeneration.States;

namespace SourceGeneration.Blux.Sample.Actions;

public class CountAction
{
}

public class DefaultActionHandler
{
    [ActionHandler]
    public static void Handle(CountAction _, State<MyState> state)
    {
        state.Update(x => x.Count++);
    }
}
