namespace PenumbraModForwarder.Common.Interfaces;

public interface IPenumbraApi
{
    public Task InstallAsync(string modPath);
}