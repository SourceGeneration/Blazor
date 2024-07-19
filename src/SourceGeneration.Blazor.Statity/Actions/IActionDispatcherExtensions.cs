using SourceGeneration.ActionDispatcher;

namespace SourceGeneration.Blazor;

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

    public static void Navigate(this IActionDispatcher dispatcher, string? uri, Dictionary<string, object?>? queryParameters, bool? forceLoad = null, bool? replace = null)
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
