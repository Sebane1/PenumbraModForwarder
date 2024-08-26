using ReactiveUI;
using System.Reactive;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IProcessHelperService _processHelperService;
    private readonly IFileWatcher _fileWatcher;
    private readonly IAssociateFileTypeService _associateFileTypeService;
    private readonly IStartupService _startupService;
    private string _selectedFolderPath;
    private bool _autoDelete;
    private bool _autoLoad;
    private bool _extractAll;
    private bool _notificationEnabled;
    private bool _selectBoxEnabled;
    private string _versionNumber;
    private bool _fileLinkingEnabled;
    private bool _runOnStartup;
    
    public bool RunOnStartup
    {
        get => _runOnStartup;
        set => this.RaiseAndSetIfChanged(ref _runOnStartup, value);
    }
    
    public bool FileLinkingEnabled
    {
        get => _fileLinkingEnabled;
        set => this.RaiseAndSetIfChanged(ref _fileLinkingEnabled, value);
    }
    
    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set => this.RaiseAndSetIfChanged(ref _selectedFolderPath, value);
    }
    
    public string VersionNumber
    {
        get => _versionNumber;
        set => this.RaiseAndSetIfChanged(ref _versionNumber, value);
    }
    
    public bool NotificationEnabled
    {
        get => _notificationEnabled;
        set => this.RaiseAndSetIfChanged(ref _notificationEnabled, value);
    }
    
    public bool AutoDelete
    {
        get => _autoDelete;
        set => this.RaiseAndSetIfChanged(ref _autoDelete, value);
    }
    
    public bool AutoLoad
    {
        get => _autoLoad;
        set
        {
            this.RaiseAndSetIfChanged(ref _autoLoad, value);
            SelectBoxEnabled = value;
        }
    }
    
    public bool ExtractAll
    {
        get => _extractAll;
        set => this.RaiseAndSetIfChanged(ref _extractAll, value);
    }
    
    public bool SelectBoxEnabled
    {
        get => _selectBoxEnabled;
        set => this.RaiseAndSetIfChanged(ref _selectBoxEnabled, value);
    }
    
    public ReactiveCommand<Unit, Unit> OpenFolderDialog { get; }
    public ReactiveCommand<bool, Unit> UpdateAutoDeleteCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateAutoLoadCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateExtractAllCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateNotificationCommand { get; }
    public ReactiveCommand<bool, Unit> EnableFileLinkingCommand { get; }
    public ReactiveCommand<bool, Unit> UpdateStartupCommand { get; }

    #region Link Buttons
    
    public ReactiveCommand<Unit, Unit> OpenXivArchiveCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenGlamourDresserCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenNexusModsCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenAetherLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenHelioSphereCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenPrettyKittyCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDiscordCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDonateCommand { get; }
    
    #endregion
    

    public MainWindowViewModel(IConfigurationService configurationService, ILogger<MainWindowViewModel> logger, IProcessHelperService processHelperService, IAssociateFileTypeService associateFileTypeService, IStartupService startupService, IFileWatcher fileWatcher)
    {
        _configurationService = configurationService;
        _logger = logger;
        _processHelperService = processHelperService;
        _associateFileTypeService = associateFileTypeService;
        _startupService = startupService;
        // This will start the file watcher
        _fileWatcher = fileWatcher;
        SetAllConfigValues();
        SetVersionNumber();
        
        _logger.LogInformation("MainWindowViewModel initialized");
        OpenFolderDialog = ReactiveCommand.Create(OpenFolder);
        UpdateAutoDeleteCommand = ReactiveCommand.Create<bool>(UpdateAutoDelete);
        UpdateAutoLoadCommand = ReactiveCommand.Create<bool>(UpdateAutoLoad);
        UpdateExtractAllCommand = ReactiveCommand.Create<bool>(UpdateExtractAll);
        UpdateNotificationCommand = ReactiveCommand.Create<bool>(UpdateNotification);
        EnableFileLinkingCommand = ReactiveCommand.Create<bool>(EnableFileLinking);
        UpdateStartupCommand = ReactiveCommand.Create<bool>(UpdateStartup);
        
        #region Link Buttons
        
        OpenXivArchiveCommand = ReactiveCommand.Create(OpenXivArchive);
        OpenGlamourDresserCommand = ReactiveCommand.Create(OpenGlamourDresser);
        OpenNexusModsCommand = ReactiveCommand.Create(OpenNexusMods);
        OpenAetherLinkCommand = ReactiveCommand.Create(OpenAetherLink);
        OpenHelioSphereCommand = ReactiveCommand.Create(OpenHelioSphere);
        OpenPrettyKittyCommand = ReactiveCommand.Create(OpenPrettyKitty);
        OpenDiscordCommand = ReactiveCommand.Create(OpenDiscord);
        OpenDonateCommand = ReactiveCommand.Create(OpenDonate);

        #endregion
    }
    
    private void SetAllConfigValues()
    {
        SelectedFolderPath = _configurationService.GetConfigValue(config => config.DownloadPath);
        AutoDelete = _configurationService.GetConfigValue(config => config.AutoDelete);
        AutoLoad = _configurationService.GetConfigValue(config => config.AutoLoad);
        ExtractAll = _configurationService.GetConfigValue(config => config.ExtractAll);
        NotificationEnabled = _configurationService.GetConfigValue(config => config.NotificationEnabled);
        FileLinkingEnabled = _configurationService.GetConfigValue(config => config.FileLinkingEnabled);
        RunOnStartup = _configurationService.GetConfigValue(config => config.StartOnBoot);
        
        RunConfigLogic();
    }

    private void RunConfigLogic()
    {
        _logger.LogInformation("Setting config logic");
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PenumbraModForwarder.exe");
        
        if (_configurationService.GetConfigValue(o => o.StartOnBoot))
        {
            _startupService.RunOnStartup();
        }

        if (_configurationService.GetConfigValue(o => o.FileLinkingEnabled))
        {
            _associateFileTypeService.AssociateFileTypes(path);
        }
    }
    
    private void SetVersionNumber()
    {
        VersionNumber = $"Version: {Application.ProductVersion.Split("+")[0]}";
    }

    #region Link Buttons

    private void OpenXivArchive()
    {
        _processHelperService.OpenXivArchive();
    }
    
    private void OpenGlamourDresser()
    {
        _processHelperService.OpenGlamourDresser();
    }
    
    private void OpenNexusMods()
    {
        _processHelperService.OpenNexusMods();
    }
    
    private void OpenAetherLink()
    {
        _processHelperService.OpenAetherLink();
    }
    
    private void OpenHelioSphere()
    {
        if (MessageBox.Show("Heliosphere requires a separate dalamud plugin to use.") == DialogResult.OK)
        {
            _processHelperService.OpenHelios();
        }
    }
    
    private void OpenPrettyKitty()
    {
        _processHelperService.OpenPrettyKitty();
    }
    
    private void OpenDiscord()
    {
        _processHelperService.OpenSupportDiscord();
    }
    
    private void OpenDonate()
    {
        _processHelperService.OpenDonate();
    }

    #endregion

    private void UpdateStartup(bool value)
    {
        _logger.LogInformation($"RunOnStartup: {value}");
        _configurationService.SetConfigValue(
            (config, startup) => config.StartOnBoot = startup, 
            value
        );
        _startupService.RunOnStartup();
    }
    
    private void UpdateNotification(bool value)
    {
        _logger.LogInformation($"NotificationEnabled: {value}");
        _configurationService.SetConfigValue(
            (config, notification) => config.NotificationEnabled = notification, 
            value
        );
    }
    
    private void UpdateAutoDelete(bool value)
    {
        _logger.LogInformation($"AutoDelete: {value}");
        _configurationService.SetConfigValue(
            (config, delete) => config.AutoDelete = delete, 
            value
        );
    }
    
    private void EnableFileLinking(bool value)
    {
        _logger.LogInformation($"FileLinkingEnabled: {value}");
        _configurationService.SetConfigValue(
            (config, linking) => config.FileLinkingEnabled = linking, 
            value
        );
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PenumbraModForwarder.exe");
        _associateFileTypeService.AssociateFileTypes(path);
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