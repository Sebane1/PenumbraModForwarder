using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Events;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.UI.ViewModels
{
    public class InstallViewModel : ViewModelBase, IDisposable
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly ILogger _logger;
        private string _currentTaskId;
        private bool _isSelectionVisible;

        public ObservableCollection<FileItemViewModel> Files { get; } = new();

        public bool IsSelectionVisible
        {
            get => _isSelectionVisible;
            set => this.RaiseAndSetIfChanged(ref _isSelectionVisible, value);
        }

        public ReactiveCommand<Unit, Unit> InstallCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public InstallViewModel(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
            _logger = Log.ForContext<InstallViewModel>();

            InstallCommand = ReactiveCommand.CreateFromTask(ExecuteInstallCommand);
            CancelCommand = ReactiveCommand.CreateFromTask(ExecuteCancelCommand);

            _webSocketClient.FileSelectionRequested += OnFileSelectionRequested;
        }

        private void OnFileSelectionRequested(object sender, FileSelectionRequestedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _currentTaskId = e.TaskId;
                Files.Clear();

                foreach (var file in e.AvailableFiles)
                {
                    var fileName = Path.GetFileName(file);
                    Files.Add(new FileItemViewModel { FileName = fileName, FilePath = file, IsSelected = true });
                    _logger.Information($"Added file {fileName}");
                }

                _logger.Information($"Selected {Files.Count} files");
                IsSelectionVisible = true;
                // TODO: We need a way to notify users who have the application out of focus
                // We can just use Impl win32 for this but not sure how to do it on Unix base systems
            });
        }

        private async Task ExecuteInstallCommand()
        {
            var selectedFiles = Files
                .Where(f => f.IsSelected)
                .Select(f => f.FilePath)
                .ToList();

            var responseMessage = new WebSocketMessage
            {
                Type = WebSocketMessageType.Status,
                TaskId = _currentTaskId,
                Status = "user_selection",
                Progress = 0,
                Message = JsonConvert.SerializeObject(selectedFiles)
            };

            await _webSocketClient.SendMessageAsync(responseMessage, "/install");

            IsSelectionVisible = false;
            _logger.Information("User selected files sent: {Files}", selectedFiles);
        }

        private async Task ExecuteCancelCommand()
        {
            IsSelectionVisible = false;
            _logger.Information("User canceled the file selection.");

            // Send empty selection
            var responseMessage = new WebSocketMessage
            {
                Type = WebSocketMessageType.Status,
                TaskId = _currentTaskId,
                Status = "user_selection",
                Progress = 0,
                Message = JsonConvert.SerializeObject(new List<string>())
            };

            await _webSocketClient.SendMessageAsync(responseMessage, "/install");
        }

        public void Dispose()
        {
            _webSocketClient.FileSelectionRequested -= OnFileSelectionRequested;
        }
    }
}