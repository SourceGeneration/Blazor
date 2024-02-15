using Microsoft.AspNetCore.Components;

namespace SourceGeneration.Blux;

public abstract class BluxLayoutComponentBase : BluxComponentBase
{
    [Parameter] public RenderFragment? Body { get; set; }
}
