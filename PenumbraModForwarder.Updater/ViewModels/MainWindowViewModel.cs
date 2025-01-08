using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Updater.Extensions;
using PenumbraModForwarder.Updater.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.Updater.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IGetBackgroundInformation _getBackgroundInformation;
    private readonly IUpdateService _updateService;
    private readonly IDownloadAndInstallUpdates _downloadAndInstallUpdates;
    private readonly IAppArguments _appArguments;
    private readonly IConfigurationService _configurationService;

    private readonly Random _random = new();
    private int _lastIndex1 = -1;
    private int _lastIndex2 = -1;

    private GithubStaticResources.InformationJson? _infoJson;
    public GithubStaticResources.InformationJson? InfoJson
    {
        get => _infoJson;
        set => this.RaiseAndSetIfChanged(ref _infoJson, value);
    }

    private GithubStaticResources.UpdaterInformationJson? _updaterInfoJson;
    public GithubStaticResources.UpdaterInformationJson? UpdaterInfoJson
    {
        get => _updaterInfoJson;
        set => this.RaiseAndSetIfChanged(ref _updaterInfoJson, value);
    }

    private string[]? _backgroundImages;
    public string[]? BackgroundImages
    {
        get => _backgroundImages;
        set => this.RaiseAndSetIfChanged(ref _backgroundImages, value);
    }

    private string? _currentImage;
    public string? CurrentImage
    {
        get => _currentImage;
        set => this.RaiseAndSetIfChanged(ref _currentImage, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    private string _currentVersion = string.Empty;
    public string CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }

    private string _updatedVersion = string.Empty;
    public string UpdatedVersion
    {
        get => _updatedVersion;
        set => this.RaiseAndSetIfChanged(ref _updatedVersion, value);
    }

    private IDisposable? _imageTimer;

    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

    private string _numberedVersionCurrent = string.Empty;
    private string _numberedVersionUpdated = string.Empty;

    public MainWindowViewModel(
        IGetBackgroundInformation getBackgroundInformation,
        IUpdateService updateService,
        IDownloadAndInstallUpdates downloadAndInstallUpdates,
        IAppArguments appArguments,
        IConfigurationService configurationService)
    {
        _logger.Debug("Constructing MainWindowViewModel...");

        _getBackgroundInformation = getBackgroundInformation;
        _updateService = updateService;
        _downloadAndInstallUpdates = downloadAndInstallUpdates;
        _appArguments = appArguments;
        _configurationService = configurationService;

        // Toggle Sentry logging based on config
        if ((bool)_configurationService.ReturnConfigValue(c => c.Common.EnableSentry))
        {
            DependencyInjection.EnableSentryLogging();
        }
        else
        {
            DependencyInjection.DisableSentryLogging();
        }

        // Determine current version
        var externalCurrentVersion = _appArguments.Args.Length > 0 
            ? _appArguments.Args[0] 
            : null;

        if (!string.IsNullOrWhiteSpace(externalCurrentVersion))
        {
            _numberedVersionCurrent = externalCurrentVersion;
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            _numberedVersionCurrent = version == null
                ? "Local Build"
                : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        CurrentVersion = $"Current Version: v{_numberedVersionCurrent}";

        UpdateCommand = ReactiveCommand.CreateFromTask(PerformUpdateAsync);

        Begin();

        StatusText = "Waiting for Update...";
    }

    private async Task PerformUpdateAsync()
    {
        try
        {
            _logger.Debug("Update button clicked");
            StatusText = "Downloading Update...";

            // Attempt to download and install
            var (success, downloadPath) = await _downloadAndInstallUpdates
                .DownloadAndInstallAsync(_numberedVersionCurrent);

            if (!success)
            {
                StatusText = "Download Failed";
                return;
            }

            // If this code is reached, the download succeeded
            StatusText = "Download Complete!";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            StatusText = "Update Failed";
        }
    }

    private async Task Begin()
    {
        _logger.Debug("Begin() called for MainWindowViewModel");

        var latestVersion = await _updateService.GetMostRecentVersionAsync();
        UpdatedVersion = $"Updated Version: {latestVersion}";
        _numberedVersionUpdated = latestVersion;

        if (!CurrentVersion.Contains(latestVersion))
        {
            StatusText = "Update Needed...";
        }

        var (info, updater) = await _getBackgroundInformation.GetResources();
        InfoJson = info;
        UpdaterInfoJson = updater;

        if (UpdaterInfoJson?.Backgrounds?.Images != null)
        {
            BackgroundImages = UpdaterInfoJson.Backgrounds.Images;
        }

        StartImageRotation();
    }

    private void StartImageRotation()
    {
        if (BackgroundImages == null || BackgroundImages.Length == 0) return;

        var initialIndex = _random.Next(BackgroundImages.Length);
        CurrentImage = BackgroundImages[initialIndex];
        _lastIndex1 = initialIndex;

        _imageTimer?.Dispose();

        _imageTimer = Observable.Interval(TimeSpan.FromSeconds(30))
            .Subscribe(_ =>
            {
                if (BackgroundImages.Length <= 2)
                {
                    CycleWithoutRandom();
                    return;
                }

                int newIndex;
                do
                {
                    newIndex = _random.Next(BackgroundImages.Length);
                }
                while (newIndex == _lastIndex1 || newIndex == _lastIndex2);

                CurrentImage = BackgroundImages[newIndex];
                _lastIndex2 = _lastIndex1;
                _lastIndex1 = newIndex;
            });
    }

    private void CycleWithoutRandom()
    {
        if (BackgroundImages == null) return;

        var currentIndex = Array.IndexOf(BackgroundImages, CurrentImage);
        var nextIndex = (currentIndex + 1) % BackgroundImages.Length;
        CurrentImage = BackgroundImages[nextIndex];
    }
}