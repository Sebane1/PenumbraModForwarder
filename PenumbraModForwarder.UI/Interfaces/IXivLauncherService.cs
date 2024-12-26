namespace PenumbraModForwarder.UI.Interfaces;

public interface IXivLauncherService
{
    void EnableAutoStart(bool enable, string appPath, string label);
    void EnableAutoStartWatchdog(bool enable);
}