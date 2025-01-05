using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.ConsoleTooling.Interfaces;
using Serilog;

namespace PenumbraModForwarder.ConsoleTooling.Services;

public class InstallingService : IInstallingService
{
    private readonly ILogger _logger;
    private readonly IModInstallService _modInstallService;
    private readonly ISoundManagerService _soundManagerService;

    public InstallingService(IModInstallService modInstallService, ISoundManagerService soundManagerService)
    {
        _modInstallService = modInstallService;
        _soundManagerService = soundManagerService;
        _logger = Log.ForContext<InstallingService>();
    }
    
    public async Task HandleFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        }

        var fileType = GetFileType(filePath);

        switch (fileType)
        {
            case FileType.ModFile:
                await HandleModFileAsync(filePath);
                break;
            default:
                throw new InvalidOperationException($"Unhandled file type: {fileType}");
        }
    }
    
    private FileType GetFileType(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (FileExtensionsConsts.ModFileTypes.Contains(fileExtension))
        {
            return FileType.ModFile;
        }

        throw new NotSupportedException($"Unsupported file extension: {fileExtension}");
    }

    private async Task HandleModFileAsync(string filePath)
    {
        _logger.Information("Handling file: {FilePath}", filePath);
        try
        {
            if (await _modInstallService.InstallModAsync(filePath))
            {
                await _soundManagerService.PlaySoundAsync(SoundType.GeneralChime);
                _logger.Information("Successfully installed mod: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to handle file: {FilePath}", filePath);
        }
    }
}