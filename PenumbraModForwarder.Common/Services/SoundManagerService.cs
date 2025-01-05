using System.Reflection;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Common.Services;

public class SoundManagerService : ISoundManagerService
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;

    private readonly Dictionary<SoundType, string> _soundFileMap = new()
    {
        [SoundType.GeneralChime] = "notification.mp3"
    };

    private readonly List<string> _embeddedResourceNames;

    public SoundManagerService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _logger = Log.ForContext<SoundManagerService>();
        _logger.Debug("Constructing SoundManagerService.");

        _embeddedResourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .ToList();

        _logger.Debug("Found {Count} embedded resources.", _embeddedResourceNames.Count);
    }

    public async Task PlaySoundAsync(SoundType soundType, float volume = 1.0f)
    {
        if (!(bool)_configurationService.ReturnConfigValue(c => c.UI.NotificationSoundEnabled)) return;
        
        const bool waitUntilFinished = true;
        
        var isMuted = IsSystemMuted();
        _logger.Debug("System muted state is {IsMuted}", isMuted);
        if (isMuted)
        {
            _logger.Information("Skipping playback because system is muted.");
            return;
        }

        _logger.Debug(
            "PlaySoundAsync called for {SoundType}. Volume={Volume}, WaitUntilFinished={WaitUntilFinished}",
            soundType,
            volume,
            waitUntilFinished
        );

        var filePath = EnsureSoundFileExtracted(soundType);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.Debug("No valid filePath resolved for {SoundType}. Exiting PlaySoundAsync.", soundType);
            return;
        }

        try
        {
            using var outputDevice = new WaveOutEvent();
            await using var audioFile = new AudioFileReader(filePath);

            _logger.Debug("AudioFileReader created for {SoundType} from path: {FilePath}", soundType, filePath);

            audioFile.Volume = volume;
            _logger.Debug("Volume set to {Volume} for {SoundType}.", volume, soundType);

            outputDevice.Init(audioFile);
            outputDevice.Play();

            _logger.Debug("Playback started for {SoundType}.", soundType);

            if (waitUntilFinished)
            {
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(100);
                }
                _logger.Debug("Playback finished for {SoundType}.", soundType);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error playing sound for {SoundType}", soundType);
        }
    }

    private string EnsureSoundFileExtracted(SoundType soundType)
    {
        if (!_soundFileMap.TryGetValue(soundType, out var fileName) || string.IsNullOrWhiteSpace(fileName))
        {
            _logger.Debug("No entry found in _soundFileMap for {SoundType}.", soundType);
            return null;
        }

        _logger.Debug(
            "Attempting to ensure sound file extracted for {SoundType} with file name {FileName}.",
            soundType,
            fileName
        );

        try
        {
            var soundFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound");
            if (!Directory.Exists(soundFolder))
            {
                Directory.CreateDirectory(soundFolder);
                _logger.Debug("Created directory: {SoundFolder}", soundFolder);
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

            _logger.Debug(
                "Extracted {FileName} resource to {DestinationPath}.",
                fileName,
                destinationPath
            );

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