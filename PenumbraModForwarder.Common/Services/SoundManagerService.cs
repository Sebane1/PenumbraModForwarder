using System.Reflection;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NLog;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class SoundManagerService : ISoundManagerService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigurationService _configurationService;
    private readonly Dictionary<SoundType, string> _soundFileMap = new()
    {
        [SoundType.GeneralChime] = "notification.mp3"
    };
    private readonly List<string> _embeddedResourceNames;

    // Simple lock object to coordinate playback
    private static readonly object PlaybackLock = new();
    private static bool _isPlaying;
    private static DateTime _lastPlayTime = DateTime.MinValue;
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(2);

    public SoundManagerService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _logger.Debug("Constructing SoundManagerService.");

        _embeddedResourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .ToList();

        _logger.Debug("Found {Count} embedded resources.", _embeddedResourceNames.Count);
    }

    public async Task PlaySoundAsync(SoundType soundType, float volume = 1.0f)
    {
        var notificationEnabled = (bool)_configurationService.ReturnConfigValue(c => c.UI.NotificationSoundEnabled);
        if (!notificationEnabled)
        {
            _logger.Debug("Notification sound is disabled in the configuration.");
            return;
        }

        var now = DateTime.UtcNow;
        if (now - _lastPlayTime < Cooldown)
        {
            _logger.Debug("Skipping playback because it is within the cooldown period.");
            return;
        }

        var isMuted = IsSystemMuted();
        _logger.Debug("System muted state is {IsMuted}", isMuted);
        if (isMuted)
        {
            _logger.Info("Skipping playback because system is muted.");
            return;
        }

        lock (PlaybackLock)
        {
            if (_isPlaying)
            {
                _logger.Debug("Another sound is currently playing. Skipping new playback to avoid overlap.");
                return;
            }
            _isPlaying = true;
            _lastPlayTime = now;
        }

        _logger.Debug("PlaySoundAsync called for {SoundType} with Volume={Volume}.", soundType, volume);

        var filePath = EnsureSoundFileExtracted(soundType);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.Debug("No valid file path found for {SoundType}. Exiting PlaySoundAsync.", soundType);
            lock (PlaybackLock)
            {
                _isPlaying = false;
            }
            return;
        }

        try
        {
            using var outputDevice = new WaveOutEvent();
            await using var audioFile = new AudioFileReader(filePath);
            audioFile.Volume = volume;

            outputDevice.Init(audioFile);
            outputDevice.Play();

            _logger.Debug("Playback started for {SoundType}.", soundType);

            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }

            _logger.Debug("Playback finished for {SoundType}.", soundType);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error playing sound for {SoundType}", soundType);
        }
        finally
        {
            lock (PlaybackLock)
            {
                _isPlaying = false;
            }
        }
    }

    private string EnsureSoundFileExtracted(SoundType soundType)
    {
        if (!_soundFileMap.TryGetValue(soundType, out var fileName) || string.IsNullOrWhiteSpace(fileName))
        {
            _logger.Debug("No entry found in soundFileMap for {SoundType}.", soundType);
            return null;
        }

        _logger.Debug("Ensuring sound file is extracted for {SoundType} with file name {FileName}.", soundType, fileName);
        try
        {
            var soundFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound");
            if (!Directory.Exists(soundFolder))
            {
                Directory.CreateDirectory(soundFolder);
                _logger.Debug("Created directory {SoundFolder}", soundFolder);
            }

            var destinationPath = Path.Combine(soundFolder, fileName);
            if (File.Exists(destinationPath))
            {
                _logger.Debug("Sound file already exists at {DestinationPath}.", destinationPath);
                return destinationPath;
            }

            var resourceName = _embeddedResourceNames
                .FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                _logger.Error("Embedded resource not found for {FileName}.", fileName);
                return null;
            }

            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                _logger.Error("Resource stream was null for {FileName}.", fileName);
                return null;
            }

            using var fileStream = File.Create(destinationPath);
            resourceStream.CopyTo(fileStream);

            _logger.Debug("Extracted {FileName} resource to {DestinationPath}.", fileName, destinationPath);

            return destinationPath;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to extract resource file {FileName}.", fileName);
            return null;
        }
    }

    private bool IsSystemMuted()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.AudioEndpointVolume.Mute;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving system mute state");
            return false;
        }
    }
}