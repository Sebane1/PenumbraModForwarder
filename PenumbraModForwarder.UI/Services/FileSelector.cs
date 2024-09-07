using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services
{
    public class FileSelector : IFileSelector
    {
        private readonly ILogger<FileSelectViewModel> _fileLogger;
        private readonly ILogger<FileSelector> _logger;
    
        public FileSelector(ILogger<FileSelectViewModel> fileLogger, ILogger<FileSelector> logger)
        {
            _fileLogger = fileLogger;
            _logger = logger;
        }

        public string[] SelectFiles(string[] files, string archiveFileName)
        {
            if (Application.OpenForms.Count > 0)
            {
                // Ensure we use the UI thread for showing the form
                return Application.OpenForms[0].Invoke(() => SelectFilesInternal(files, archiveFileName));
            }

            return SelectFilesInternal(files, archiveFileName);
        }

        private string[] SelectFilesInternal(string[] files, string archiveName)
        {
            using var fileSelectForm = new FileSelect(new FileSelectViewModel(_fileLogger));
            fileSelectForm.ViewModel.LoadFiles(files);
            fileSelectForm.ViewModel.ArchiveFileName = archiveName;
            
            var result = fileSelectForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                _logger.LogInformation("Files selected: {0}", string.Join(", ", fileSelectForm.ViewModel.SelectedFiles));
                return fileSelectForm.ViewModel.SelectedFiles;
            }

            _logger.LogWarning("File selection was canceled.");
            return [];
        }
    }
}