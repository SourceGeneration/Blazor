using Microsoft.AspNetCore.Components;

namespace SourceGeneration.Blazor;

public abstract class StateLayoutComponentBase : StateComponentBase
{
    [Parameter] public RenderFragment? Body { get; set; }
}
