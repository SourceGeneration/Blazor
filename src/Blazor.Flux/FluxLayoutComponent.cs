using Microsoft.AspNetCore.Components;

namespace Blazor.Flux;

public abstract class FluxLayoutComponent : FluxComponent
{
    [Parameter] public RenderFragment? Body { get; set; }
}
