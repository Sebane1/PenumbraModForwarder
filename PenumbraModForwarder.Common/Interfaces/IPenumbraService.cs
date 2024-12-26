namespace PenumbraModForwarder.Common.Interfaces;

public interface IPenumbraService
{
    void InitializePenumbraPath();
    string InstallMod(string sourceFilePath);
}