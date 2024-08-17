using PenumbraModForwarder.Common.Interfaces;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;

namespace PenumbraModForwarder.Common.Services
{
    public class ArchiveHelperService : IArchiveHelperService
    {
        private readonly ILogger<ArchiveHelperService> _logger;

        public ArchiveHelperService(ILogger<ArchiveHelperService> logger)
        {
            _logger = logger;
        }

        public void ExtractArchive(string filePath)
        {
            var files = GetFilesInArchive(filePath);
            if (files.Length > 1)
            {
                _logger.LogInformation("Multiple files found in archive. Showing file selection dialog.");
            }
        }

        private string[] GetFilesInArchive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            var allowedExtensions = new[] {".pmp", ".ttmp2", ".ttmp", ".rpvsp"};
            var fileEntries = new HashSet<string>();

            using (var archive = OpenArchive(filePath))
            {
                if (archive == null) throw new InvalidOperationException("Archive could not be opened.");

                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    var extension = Path.GetExtension(entry.Key).ToLower();
                    if (allowedExtensions.Contains(extension))
                    {
                        fileEntries.Add(entry.Key);
                    }
                }
            }

            return fileEntries.ToArray();
        }

        private IArchive OpenArchive(string filePath)
        {
            return SevenZipArchive.Open(filePath);
        }
    }
}
