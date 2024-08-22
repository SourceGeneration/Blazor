using SourceGeneration.ActionDispatcher;
using SourceGeneration.Blazor.Sample.States;

namespace SourceGeneration.Blazor.Sample.Actions;

public class DefaultActionHandler
{
    [ActionHandler]
    public static void Handle(IncrementAction _, MyState state)
    {
        state.Count++;
        state.AcceptChanges();
    }
}
