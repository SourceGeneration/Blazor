using SourceGeneration.ActionDispatcher;
using SourceGeneration.Blux.Sample.States;

namespace SourceGeneration.Blux.Sample.Actions;

public class DefaultActionHandler
{
    [ActionHandler]
    public static void Handle(IncrementAction _, MyState state)
    {
        state.Count++;
        state.AcceptChanges();
    }
}
