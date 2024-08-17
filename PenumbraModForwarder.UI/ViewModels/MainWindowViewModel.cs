using ReactiveUI;
using System.Reactive;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<MainWindowViewModel> _logger;
    
    private string _selectedFolderPath;
    private bool _autoDelete;
    private bool _autoLoad;
    private bool _extractAll;
    
    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set => this.RaiseAndSetIfChanged(ref _selectedFolderPath, value);
    }
    
    public bool AutoDelete
    {
        get => _autoDelete;
        set => this.RaiseAndSetIfChanged(ref _autoDelete, value);
    }
    
    public bool AutoLoad
    {
        get => _autoLoad;
        set => this.RaiseAndSetIfChanged(ref _autoLoad, value);
    }
    
    public bool ExtractAll
    {
        get => _extractAll;
        set => this.RaiseAndSetIfChanged(ref _extractAll, value);
    }
    
    public ReactiveCommand<Unit, Unit> OpenFolderDialog { get; }
    public ReactiveCommand<bool, Unit> UpdateAutoDeleteCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateAutoLoadCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateExtractAllCommand { get; }
    

    public MainWindowViewModel(IConfigurationService configurationService, ILogger<MainWindowViewModel> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
        SetAllConfigValues();
        
        _logger.LogInformation("MainWindowViewModel created.");
        OpenFolderDialog = ReactiveCommand.Create(OpenFolder);
        UpdateAutoDeleteCommand = ReactiveCommand.Create<bool>(UpdateAutoDelete);
        UpdateAutoLoadCommand = ReactiveCommand.Create<bool>(UpdateAutoLoad);
        UpdateExtractAllCommand = ReactiveCommand.Create<bool>(UpdateExtractAll);
    }
    
    private void SetAllConfigValues()
    {
        SelectedFolderPath = _configurationService.GetConfigValue(config => config.DownloadPath);
        AutoDelete = _configurationService.GetConfigValue(config => config.AutoDelete);
        AutoLoad = _configurationService.GetConfigValue(config => config.AutoLoad);
        ExtractAll = _configurationService.GetConfigValue(config => config.ExtractAll);
    }
    
    private void UpdateAutoDelete(bool value)
    {
        _logger.LogInformation($"AutoDelete: {value}");
        _configurationService.SetConfigValue(
            (config, delete) => config.AutoDelete = delete, 
            value
        );
    }
    
    private void UpdateAutoLoad(bool value)
    {
        _logger.LogInformation($"AutoLoad: {value}");
        _configurationService.SetConfigValue(
            (config, forward) => config.AutoLoad = forward, 
            value
        );
    }
    
    private void UpdateExtractAll(bool value)
    {
        _logger.LogInformation($"ExtractAll: {value}");
        _configurationService.SetConfigValue(
            (config, extract) => config.ExtractAll = extract, 
            value
        );
    }
    
    private void OpenFolder()
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() != DialogResult.OK) return;
        SelectedFolderPath = dialog.SelectedPath;
        
        _logger.LogInformation($"Selected folder: {SelectedFolderPath}");
            
        _configurationService.SetConfigValue(
            (config, path) => config.DownloadPath = path, 
            SelectedFolderPath
        );
    }
}