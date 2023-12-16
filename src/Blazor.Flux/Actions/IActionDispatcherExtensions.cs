using Sg.ActionDispatcher;

namespace Blazor.Flux;

public static class IActionDispatcherExtensions
{
    public static void Navigate(this IActionDispatcher dispatcher, string? uri, bool? forceLoad = null, bool? replace = null)
    {
        dispatcher.Dispatch(new NavigateAction
        {
            Uri = uri,
            ForceLoad = forceLoad,
            Replace = replace
        });
    }

    public static void Navigate(this IActionDispatcher dispatcher, string? uri, Dictionary<string, object?>? queryParameters = null, bool? forceLoad = null, bool? replace = null)
    {
        dispatcher.Dispatch(new NavigateAction
        {
            Uri = uri,
            QueryParameters = queryParameters,
            ForceLoad = forceLoad,
            Replace = replace
        });
    }
}
