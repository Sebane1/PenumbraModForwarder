namespace PenumbraModForwarder.Common.Interfaces;

public interface IPenumbraInstallerService
{
    public Task<bool> InstallMod(string modPath);
}