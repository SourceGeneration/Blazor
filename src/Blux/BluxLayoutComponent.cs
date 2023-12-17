using Microsoft.AspNetCore.Components;

namespace Sg.Blux;

public abstract class BluxLayoutComponent : BluxComponent
{
    [Parameter] public RenderFragment? Body { get; set; }
}
