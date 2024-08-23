namespace PenumbraModForwarder.Common.Interfaces;

public interface IPenumbraApi
{
    public Task<bool> InstallAsync(string modPath);
}