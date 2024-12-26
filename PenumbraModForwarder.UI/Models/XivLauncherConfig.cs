namespace PenumbraModForwarder.UI.Models;

public class LauncherConfig
{
    public string AddonList { get; set; }
}

public class AddonEntry
{
    public bool IsEnabled { get; set; }
    public AddonInfo Addon { get; set; }
}

public class AddonInfo
{
    public string Path { get; set; }
    public string CommandLine { get; set; }
    public bool RunAsAdmin { get; set; }
    public bool RunOnClose { get; set; }
    public bool KillAfterClose { get; set; }
    public string Name { get; set; }
}