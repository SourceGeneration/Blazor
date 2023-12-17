namespace Sg.Blux;

public class NavigateAction
{
    public string? Uri { get; init; }
    public IReadOnlyDictionary<string, object?>? QueryParameters { get; init; }
    public bool? ForceLoad { get; init; }
    public bool? Replace { get; init; }
}
