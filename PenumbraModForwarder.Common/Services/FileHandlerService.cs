using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class FileHandlerService : IFileHandlerService
{
    private readonly ILogger<FileHandlerService> _logger;
    private readonly IArchiveHelperService _archiveHelperService;
    private readonly IPenumbraInstallerService _penumbraInstallerService;
    private readonly IConfigurationService _configurationService;
    private readonly IErrorWindowService _errorWindowService;
    private readonly IArkService _arkService;

    public FileHandlerService(ILogger<FileHandlerService> logger, IArchiveHelperService archiveHelperService, IPenumbraInstallerService penumbraInstallerService, IConfigurationService configurationService, IErrorWindowService errorWindowService, IArkService arkService)
    {
        _logger = logger;
        _archiveHelperService = archiveHelperService;
        _penumbraInstallerService = penumbraInstallerService;
        _configurationService = configurationService;
        _errorWindowService = errorWindowService;
        _arkService = arkService;
    }

    public void HandleFile(string filePath)
    {
        _logger.LogInformation($"Handling file: {filePath}");

        if (IsArchive(filePath))
        {
            _logger.LogInformation($"File '{filePath}' is an archive. Initiating extraction.");
            _archiveHelperService.QueueExtractionAsync(filePath);
        }
        else if (IsModFile(filePath))
        {
            _logger.LogInformation($"File '{filePath}' is a mod file. Installing mod.");
            try
            {
                _penumbraInstallerService.InstallMod(filePath);
                _logger.LogInformation($"Mod file '{filePath}' installed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to install mod: {filePath}");
                _errorWindowService.ShowError($"Failed to install mod: {filePath}");
            }
        }
        else if (IsRPVSFile(filePath))
        {
            _logger.LogInformation($"File '{filePath}' is a RolePlayVoice file. Installing file.");
            try
            {
                _arkService.InstallArkFile(filePath);
                _logger.LogInformation($"RolePlayVoice file '{filePath}' installed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to install RolePlayVoice file: {filePath}");
                _errorWindowService.ShowError($"Failed to install RolePlayVoice file: {filePath}");
            }
        }
        else
        {
            _logger.LogWarning($"File '{filePath}' does not match any known types (archive, mod, or RolePlayVoice file).");
        }
    }

    public void CleanUpTempFiles()
    {
        _logger.LogInformation("Starting cleanup of temporary files.");

        if (_configurationService.GetConfigValue(config => config.AutoDelete) == false)
        {
            _logger.LogInformation("Temp files cleanup is disabled by configuration.");
            return;
        }

        var extractionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Extraction";
        var dtConversionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\DTConversion";
        var registryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Registry";

        try
        {
            CleanDirectory(extractionPath);
            CleanDirectory(dtConversionPath);
            CleanDirectory(registryPath);

            var downloadPath = _configurationService.GetConfigValue(config => config.DownloadPath);
            _logger.LogInformation($"Checking for mod files in download path: {downloadPath}");

            if (Directory.Exists(downloadPath))
            {
                foreach (var file in Directory.GetFiles(downloadPath))
                {
                    if (IsModFile(file))
                    {
                        _logger.LogDebug($"Deleting mod file: {file}");
                        File.Delete(file);
                    }
                }
            }

            _logger.LogInformation("Temp files cleaned up successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cleaning up temp files.");
            _errorWindowService.ShowError(e.ToString());
        }
    }

    private void CleanDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            _logger.LogInformation($"Cleaning directory: {path}");
            foreach (var file in Directory.GetFiles(path))
            {
                _logger.LogDebug($"Deleting file: {file}");
                File.Delete(file);
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                _logger.LogDebug($"Deleting directory: {dir}");
                Directory.Delete(dir, true);
            }
        }
        else
        {
            _logger.LogInformation($"Directory '{path}' does not exist, skipping cleanup.");
        }
    }

    private bool IsArchive(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        _logger.LogInformation($"Checking if file '{filePath}' is an archive with extension: {extension}");

        var allowedExtensions = new[] { ".zip", ".rar", ".7z" };
        return allowedExtensions.Contains(extension);
    }

    private bool IsModFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        _logger.LogInformation($"Checking if file '{filePath}' is a mod file with extension: {extension}");

        var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp" };
        return allowedExtensions.Contains(extension);
    }

    private bool IsRPVSFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        _logger.LogInformation($"Checking if file '{filePath}' is a RolePlayVoice file with extension: {extension}");

        var allowedExtensions = new[] { ".rpvsp" };
        return allowedExtensions.Contains(extension);
    }
}
