using SourceGeneration.ActionDispatcher;
using SourceGeneration.Blux.Sample.States;
using SourceGeneration.States;

namespace SourceGeneration.Blux.Sample.Actions;

public class DefaultActionHandler
{
    [ActionHandler]
    public static void Handle(IncrementAction _, State<MyState> state)
    {
        state.Update(x => x.Count++);
    }
}
