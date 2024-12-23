using PenumbraModForwarder.BackgroundWorker.Enums;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services
{
    public class ModHandlerService : IModHandlerService
    {
        private readonly ILogger _logger;

        public ModHandlerService()
        {
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
        }
    }
}