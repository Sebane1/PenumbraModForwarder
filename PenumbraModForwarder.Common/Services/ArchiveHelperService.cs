using PenumbraModForwarder.Common.Interfaces;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.Common.Services
{
    public class ArchiveHelperService : IArchiveHelperService
    {
        private readonly ILogger<ArchiveHelperService> _logger;
        private readonly IUserInteractionService _userInteractionService;

        public ArchiveHelperService(ILogger<ArchiveHelperService> logger, IUserInteractionService userInteractionService)
        {
            _logger = logger;
            _userInteractionService = userInteractionService;
        }

        public void ExtractArchive(string filePath)
        {
            var files = GetFilesInArchive(filePath);
            if (files.Length > 1)
            {
                var selectedFile = _userInteractionService.ShowFileSelectionDialog(files);
                if (selectedFile != null)
                {
                    _logger.LogInformation($"User selected file: {selectedFile}");
                    // Proceed with the selected file
                }
            }
        }

        private string[] GetFilesInArchive(string filePath)
        {
            using var archive = ZipFile.OpenRead(filePath);
            var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };
            return archive.Entries
                .Where(entry => allowedExtensions.Contains(Path.GetExtension(entry.FullName)))
                .Select(entry => entry.FullName)
                .ToArray();
        }
    }
}