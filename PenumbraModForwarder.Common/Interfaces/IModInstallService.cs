namespace PenumbraModForwarder.Common.Interfaces;

public interface IModInstallService
{
    /// <summary>
    /// Sends a request to install a mod at the specified path.
    /// </summary>
    /// <param name="path">The path of the mod to install.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ModInstallException">Thrown when the mod installation fails.</exception>
    Task<bool> InstallModAsync(string path);
}