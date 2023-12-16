using Microsoft.AspNetCore.Components;
using Sg.ActionDispatcher;

namespace Blazor.Flux;

public class DefaultActionHandler(NavigationManager navigationManager)
{
    [ActionHandler]
    public void HandleNavigate(NavigateAction action)
    {
        var uri = action.Uri ?? navigationManager.Uri;
        if (action.QueryParameters != null)
        {
            uri = navigationManager.GetUriWithQueryParameters(uri, action.QueryParameters);
        }

        navigationManager.NavigateTo(uri, action.ForceLoad.GetValueOrDefault(false), action.Replace.GetValueOrDefault(false));
    }
}