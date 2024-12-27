using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using SevenZipExtractor;

namespace PenumbraModForwarder.Common.Services;

public class PenumbraService : IPenumbraService
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger _logger;

    private static readonly string[] PenumbraJsonLocations =
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "pluginConfigs", "Penumbra.json"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncherCN", "pluginConfigs", "Penumbra.json")
    };

    public PenumbraService(IConfigurationService configurationService, IFileStorage fileStorage)
    {
        _configurationService = configurationService;
        _fileStorage = fileStorage;
        _logger = Log.ForContext<PenumbraService>();
    }

    /// <summary>
    /// If no path is already configured, attempts to locate and store the Penumbra mod directory.
    /// </summary>
    public void InitializePenumbraPath()
    {
        var existingPath = _configurationService.ReturnConfigValue(
            c => c.BackgroundWorker.PenumbraModFolderPath
        ) as string;

        if (!string.IsNullOrWhiteSpace(existingPath))
        {
            _logger.Information("Penumbra path is already set in the configuration.");
            return;
        }

        var foundPath = FindPenumbraPath();
        if (!string.IsNullOrWhiteSpace(foundPath))
        {
            UpdatePenumbraPathInConfiguration(foundPath);
        }
        else
        {
            _logger.Warning("Penumbra path could not be located with any known configuration.");
        }
    }

    /// <summary>
    /// Always treats the specified file as an archive. Extracts to a folder named after "meta.json" 'Name' if found,
    /// otherwise uses the archive base name. Returns the final folder path where files are extracted.
    /// </summary>
    public string InstallMod(string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentNullException(nameof(sourceFilePath));

        var penumbraPath = _configurationService.ReturnConfigValue(
            c => c.BackgroundWorker.PenumbraModFolderPath
        ) as string;

        if (string.IsNullOrEmpty(penumbraPath))
            throw new InvalidOperationException("Penumbra path not configured. Make sure to set it first.");

        if (!_fileStorage.Exists(penumbraPath))
            _fileStorage.CreateDirectory(penumbraPath);
        
        using var archive = new ArchiveFile(sourceFilePath);
        
        var metaEntry = archive.Entries.FirstOrDefault(
            e => e?.FileName?.Equals("meta.json", StringComparison.OrdinalIgnoreCase) == true
        );

        var destinationFolderName = Path.GetFileNameWithoutExtension(sourceFilePath);

        if (metaEntry != null)
        {
            // Extract only meta.json to a temp file
            var tempMetaFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            archive.Extract(entry =>
            {
                if (ReferenceEquals(entry, metaEntry))
                    return tempMetaFilePath;
                return null;
            });

            try
            {
                var metaContent = _fileStorage.Read(tempMetaFilePath);
                var meta = JsonConvert.DeserializeObject<PmpMeta>(metaContent);
                if (!string.IsNullOrWhiteSpace(meta?.Name))
                {
                    destinationFolderName = meta.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to parse meta.json in {SourceFile}; using default folder name.", sourceFilePath);
            }
            finally
            {
                if (_fileStorage.Exists(tempMetaFilePath))
                    _fileStorage.Delete(tempMetaFilePath);
            }
        }
        else
        {
            _logger.Warning("No meta.json found in {SourceFile}; using default folder name.", sourceFilePath);
        }

        // Clean up invalid characters for the file system
        destinationFolderName = RemoveInvalidPathChars(destinationFolderName);

        // Create final destination folder
        var destinationFolderPath = Path.Combine(penumbraPath, destinationFolderName);
        if (!_fileStorage.Exists(destinationFolderPath))
            _fileStorage.CreateDirectory(destinationFolderPath);

        // Extract all contents to the final folder
        archive.Extract(entry =>
        {
            if (entry == null)
                return null;
            var outFileName = Path.Combine(destinationFolderPath, entry.FileName ?? string.Empty);
            return outFileName;
        });

        // Optionally remove the original archive
        // _fileStorage.Delete(sourceFilePath);

        _logger.Information("Installed archive from {Source} into {Destination}", sourceFilePath, destinationFolderPath);
        
        return destinationFolderPath;
    }

    private string FindPenumbraPath()
    {
        foreach (var location in PenumbraJsonLocations)
        {
            if (_fileStorage.Exists(location))
            {
                var path = ExtractPathFromJson(location);
                if (!string.IsNullOrWhiteSpace(path))
                    return path;
            }
        }
        return string.Empty;
    }

    private string ExtractPathFromJson(string jsonFilePath)
    {
        var fileContent = _fileStorage.Read(jsonFilePath);
        var penumbraData = JsonConvert.DeserializeObject<PenumbraModPath>(fileContent);
        return penumbraData?.ModDirectory ?? string.Empty;
    }

    private void UpdatePenumbraPathInConfiguration(string foundPath)
    {
        _logger.Information("Setting Penumbra path to {FoundPath}", foundPath);
        _configurationService.UpdateConfigValue(
            config => config.BackgroundWorker.PenumbraModFolderPath = foundPath,
            "BackgroundWorker.PenumbraModPath",
            foundPath
        );
    }

    private static string RemoveInvalidPathChars(string text)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(text.Where(ch => !invalidChars.Contains(ch)).ToArray());
    }
}