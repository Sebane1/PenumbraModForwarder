using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels
{
    public class FileSelectViewModel : ReactiveObject
    {
        private readonly ILogger<FileSelectViewModel> _logger;
        public ObservableCollection<string> Files { get; }

        private string _selectedFile;
        public string SelectedFile
        {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public ReactiveCommand<Unit, Unit> ConfirmSelectionCommand { get; }

        public FileSelectViewModel(ILogger<FileSelectViewModel> logger)
        {
            _logger = logger;
            Files = new ObservableCollection<string>();
            ConfirmSelectionCommand = ReactiveCommand.Create(ConfirmSelection);
        }

        public void LoadFiles(IEnumerable<string> files)
        {
            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(file);
            }
        }

        private void ConfirmSelection()
        {
            // Logic to confirm file selection
            _logger.LogInformation($"Selected file: {SelectedFile}");
        }
    }
}