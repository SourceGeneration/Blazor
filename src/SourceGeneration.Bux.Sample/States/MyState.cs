using SourceGeneration.ChangeTracking;

namespace SourceGeneration.Blux.Sample.States;

[ChangeTracking]
public partial class MyState : State<MyState>
{
    public MyState()
    {
        List = [];
    }

    public partial int Count { get; set; }

    public partial ChangeTrackingList<ViewItem> List { get; set; }
}

[ChangeTracking]
public partial class ViewItem
{
    public partial int Value { get; set; }
}