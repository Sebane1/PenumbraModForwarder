namespace PenumbraModForwarder.Common.Models;

public record ModInstallData(string Path)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModInstallData"/> class with an empty path.
    /// </summary>
    public ModInstallData() : this(string.Empty) { }
}