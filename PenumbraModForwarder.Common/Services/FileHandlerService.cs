using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class FileHandlerService : IFileHandlerService
{
    private readonly ILogger<FileHandlerService> _logger;
    private readonly IArchiveHelperService _archiveHelperService;

    public FileHandlerService(ILogger<FileHandlerService> logger, IArchiveHelperService archiveHelperService)
    {
        _logger = logger;
        _archiveHelperService = archiveHelperService;
    }
    
    public void HandleFile(string filePath)
    {
        //Check if the file is an archive
        if (IsArchive(filePath))
        {
            _logger.LogWarning("File is an Archive");
            // Extract the archive using ArchiveHelperService
            _archiveHelperService.ExtractArchive(filePath);
        }
        
        //Check if the file is a mod file
        if (IsModFile(filePath))
        {
            _logger.LogWarning("File is a Mod File");
        }
        
        //Check if the file is a RolePlayVoice File
        if (IsRPVSFile(filePath))
        {
            _logger.LogWarning("File is a RolePlayVoice File");
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