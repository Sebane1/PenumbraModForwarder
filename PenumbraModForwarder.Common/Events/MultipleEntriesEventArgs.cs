namespace PenumbraModForwarder.Common.Events;

public class MultipleEntriesEventArgs : EventArgs
{
    public IReadOnlyList<string> EntryNames { get; }

    public MultipleEntriesEventArgs(IReadOnlyList<string> entryNames)
    {
        EntryNames = entryNames;
    }
}