using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using Newtonsoft.Json;
using NLog;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Events;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class InstallViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IWebSocketClient _webSocketClient;
    private readonly ISoundManagerService _soundManagerService;

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

    public InstallViewModel(
        IWebSocketClient webSocketClient,
        ISoundManagerService soundManagerService)
    {
        _webSocketClient = webSocketClient;
        _soundManagerService = soundManagerService;

        InstallCommand = ReactiveCommand.CreateFromTask(ExecuteInstallCommand);
        CancelCommand = ReactiveCommand.CreateFromTask(ExecuteCancelCommand);

        _webSocketClient.FileSelectionRequested += OnFileSelectionRequested;
    }

    private void OnFileSelectionRequested(object sender, FileSelectionRequestedEventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            _currentTaskId = e.TaskId;
            Files.Clear();

            foreach (var file in e.AvailableFiles)
            {
                var fileName = Path.GetFileName(file);
                Files.Add(new FileItemViewModel
                {
                    FileName = fileName,
                    FilePath = file,
                    IsSelected = true
                });

                _logger.Info("Added file {FileName}", fileName);
            }

            _logger.Info("Selected {FileCount} files", Files.Count);

            IsSelectionVisible = true;

            await _soundManagerService.PlaySoundAsync(
                SoundType.GeneralChime,
                volume: 1.0f
            );
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
        _logger.Info("User selected files sent: {SelectedFiles}", selectedFiles);
    }

    private async Task ExecuteCancelCommand()
    {
        IsSelectionVisible = false;
        _logger.Info("User canceled the file selection.");

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