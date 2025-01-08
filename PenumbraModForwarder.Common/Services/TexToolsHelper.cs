using NLog;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class TexToolsHelper : ITexToolsHelper
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IRegistryHelper _registryHelper;
    private readonly IConfigurationService _configurationService;
    private readonly IFileSystemHelper _fileSystemHelper;

    public TexToolsHelper(
        IRegistryHelper registryHelper,
        IConfigurationService configurationService,
        IFileSystemHelper fileSystemHelper)
    {
        _registryHelper = registryHelper;
        _configurationService = configurationService;
        _fileSystemHelper = fileSystemHelper;
    }

    /// <summary>
    /// Sets or retrieves the TexTools console path in the configuration.
    /// Works on Windows, Linux, and macOS.
    /// </summary>
    /// <returns>
    /// TexToolsStatus indicating the result:
    /// - AlreadyConfigured: Path already exists in configuration
    /// - Found: Successfully found and configured new path
    /// - NotFound: ConsoleTools executable not found at expected location
    /// - NotInstalled: TexTools installation not found
    /// </returns>
    /// <remarks>
    /// Checks configuration first, then standard installation paths based on OS.
    /// If found, validates ConsoleTools executable exists and updates configuration.
    /// </remarks>
    public TexToolsStatus SetTexToolConsolePath()
    {
        // Check if the TexTool path is already configured and valid
        var configuredPath = (string)_configurationService.ReturnConfigValue(model => model.BackgroundWorker.TexToolPath);
        if (!string.IsNullOrEmpty(configuredPath) && _fileSystemHelper.FileExists(configuredPath))
        {
            _logger.Info("TexTools path already configured: {Path}", configuredPath);
            return TexToolsStatus.AlreadyConfigured;
        }

        // Attempt to find TexTools installation
        var consoleToolPath = FindTexToolsConsolePath();
        if (string.IsNullOrEmpty(consoleToolPath))
        {
            _logger.Warn("TexTools installation not found");
            return TexToolsStatus.NotInstalled;
        }

        if (!_fileSystemHelper.FileExists(consoleToolPath))
        {
            _logger.Warn("ConsoleTools executable not found at: {Path}", consoleToolPath);
            return TexToolsStatus.NotFound;
        }

        // Update configuration with the found path
        _configurationService.UpdateConfigValue(
            config => config.BackgroundWorker.TexToolPath = consoleToolPath,
            "BackgroundWorker.TexToolPath",
            consoleToolPath
        );

        _logger.Info("Successfully configured TexTools path: {Path}", consoleToolPath);
        return TexToolsStatus.Found;
    }

    private string FindTexToolsConsolePath()
    {
        string consoleToolPath = null;

        if (_registryHelper.IsRegistrySupported)
        {
            // Try to get the path from the registry (Windows)
            var path = _registryHelper.GetTexToolRegistryValue();
            if (!string.IsNullOrEmpty(path))
            {
                // Remove surrounding quotes if present
                path = path.Trim('"');

                consoleToolPath = Path.Combine(path, "FFXIV_TexTools", "ConsoleTools.exe");
                if (_fileSystemHelper.FileExists(consoleToolPath))
                {
                    return consoleToolPath;
                }
            }
        }

        // Check standard installation paths based on OS
        var standardPaths = _fileSystemHelper.GetStandardTexToolsPaths();
        foreach (var standardPath in standardPaths)
        {
            if (_fileSystemHelper.FileExists(standardPath))
            {
                return standardPath;
            }
        }

        // TexTools not found
        return null;
    }
}