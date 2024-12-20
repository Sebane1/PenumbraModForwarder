namespace PenumbraModForwarder.Common.Interfaces;

public interface IRegistryHelper
{
    /// <summary>
    /// Indicates whether the registry is supported on the current platform.
    /// </summary>
    bool IsRegistrySupported { get; }

    /// <summary>
    /// Retrieves the TexTools installation location from the Windows Registry.
    /// </summary>
    /// <returns>The installation path of TexTools, or null if not found.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when called on non-Windows platforms.</exception>
    string GetTexToolRegistryValue();
}