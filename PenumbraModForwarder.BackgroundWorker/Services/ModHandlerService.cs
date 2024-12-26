using PenumbraModForwarder.BackgroundWorker.Enums;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services
{
    public class ModHandlerService : IModHandlerService
    {
        private readonly ILogger _logger;
        private readonly IModInstallService _modInstallService;
        private readonly IWebSocketServer _webSocketServer;

        public ModHandlerService(IModInstallService modInstallService, IWebSocketServer webSocketServer)
        {
            _modInstallService = modInstallService;
            _webSocketServer = webSocketServer;
            _logger = Log.ForContext<ModHandlerService>();
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
                    _logger.Information("Successfully installed mod: {FilePath}", filePath);
                    var fileName = Path.GetFileName(filePath);
                    var taskId = Guid.NewGuid().ToString();
                    var message = WebSocketMessage.CreateStatus(taskId, "Installed File", $"Installed mod: {fileName}");
                    _webSocketServer.BroadcastToEndpointAsync("/status", message).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to handle file: {FilePath}", filePath);
                var taskId = Guid.NewGuid().ToString();
                var errorMessage = WebSocketMessage.CreateStatus(taskId, "Failed to handle file", $"Failed to handle file: {filePath}");
                _webSocketServer.BroadcastToEndpointAsync("/status", errorMessage).GetAwaiter().GetResult();
            }
        }
    }
}