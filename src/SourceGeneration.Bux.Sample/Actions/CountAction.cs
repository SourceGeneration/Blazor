using Blazor.Flux.Sample.States;
using SourceGeneration.ActionDispatcher;
using SourceGeneration.States;

namespace Blazor.Flux.Sample.Actions
{
    public class CountAction
    {
    }

    public class DefaultActionHandler
    {
        [ActionHandler]
        public static void Handle(CountAction _,IStore<MyState> state)
        {
            state.Update(x => x.Count++);
        }
    }
}
