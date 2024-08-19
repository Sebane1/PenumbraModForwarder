using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class FileHandlerService : IFileHandlerService
{
    private readonly ILogger<FileHandlerService> _logger;
    private readonly IArchiveHelperService _archiveHelperService;
    private readonly IPenumbraInstallerService _penumbraInstallerService;

    public FileHandlerService(ILogger<FileHandlerService> logger, IArchiveHelperService archiveHelperService, IPenumbraInstallerService penumbraInstallerService)
    {
        _logger = logger;
        _archiveHelperService = archiveHelperService;
        _penumbraInstallerService = penumbraInstallerService;
    }
    
    public void HandleFile(string filePath)
    {
        //Check if the file is an archive
        if (IsArchive(filePath))
        {
            _logger.LogInformation("File is an Archive");
            // Extract the archive using ArchiveHelperService
            _archiveHelperService.ExtractArchive(filePath);
        }
        
        //Check if the file is a mod file
        if (IsModFile(filePath))
        {
            _logger.LogInformation("File is a Mod File");
            // Install the mod using PenumbraInstallerService
            _penumbraInstallerService.InstallMod(filePath);
        }
        
        //Check if the file is a RolePlayVoice File
        if (IsRPVSFile(filePath))
        {
            _logger.LogInformation("File is a RolePlayVoice File");
        }
    }

    public void CleanUpTempFiles()
    {
        var _extractionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Extraction";
        var _dtConversionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\DTConversion";

        try
        {
            if (Directory.Exists(_extractionPath))
            {
                // Delete all files and folders in the extraction folder
                foreach (var file in Directory.GetFiles(_extractionPath))
                {
                    File.Delete(file);
                }
                foreach (var dir in Directory.GetDirectories(_extractionPath))
                {
                    Directory.Delete(dir, true);
                }
            }
        
            if (Directory.Exists(_dtConversionPath))
            {
                // Delete all files and folders in the dtConversion folder
                foreach (var file in Directory.GetFiles(_dtConversionPath))
                {
                    File.Delete(file);
                }
                foreach (var dir in Directory.GetDirectories(_dtConversionPath))
                {
                    Directory.Delete(dir, true);
                }
            }

            _logger.LogInformation("Temp files cleaned up successfully");
        } 
        catch (Exception e)
        {
            _logger.LogError($"Error cleaning up temp files: {e.Message}");
        }
    }
    
    private bool IsArchive(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        var allowedExtensions = new[] { ".zip", ".rar", ".7z" };
        return allowedExtensions.Contains(extension);
    }
    
    private bool IsModFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp" };
        return allowedExtensions.Contains(extension);
    }
    
    private bool IsRPVSFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        var allowedExtensions = new[] { ".rpvsp" };
        return allowedExtensions.Contains(extension);
    }
}