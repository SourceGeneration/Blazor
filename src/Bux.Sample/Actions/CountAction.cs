using Sg.ActionDispatcher;

namespace Blazor.Flux.Sample.Actions
{
    public class CountAction
    {
    }

    public class DefaultActionHandler
    {
        [ActionHandler]
        public static void Handle(CountAction _)
        {

        }
    }
}
