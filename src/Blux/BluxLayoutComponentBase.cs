using Microsoft.AspNetCore.Components;

namespace Blux;

public abstract class BluxLayoutComponentBase : BluxComponentBase
{
    [Parameter] public RenderFragment? Body { get; set; }
}
