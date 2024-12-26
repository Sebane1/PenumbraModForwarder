namespace PenumbraModForwarder.Common.Models;

public record ModReloadData(string Path, string Name)
{
    /// <summary>
    /// Initializes a new instance of this record with empty defaults.
    /// </summary>
    public ModReloadData()
        : this(string.Empty, string.Empty)
    {
    }
}