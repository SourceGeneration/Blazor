using SourceGeneration.ChangeTracking;

namespace SourceGeneration.Blux.Sample.States;

[ChangeTracking]
public class MyState : State<MyState>
{
    public virtual int Count { get; set; }

    public virtual ChangeTrackingList<ViewItem> List { get; set; } = [];
}

[ChangeTracking]
public class ViewItem
{
    public virtual int Value { get; set; }
}