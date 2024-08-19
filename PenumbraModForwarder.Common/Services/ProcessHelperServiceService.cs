using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class ProcessHelperServiceService : IProcessHelperService
{
    private readonly ILogger<ProcessHelperServiceService> _logger;

    public ProcessHelperServiceService(ILogger<ProcessHelperServiceService> logger)
    {
        _logger = logger;
    }

    private void OpenUrl(string url, string logMessage)
    {
        _logger.LogInformation(logMessage);
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void OpenLogFolder()
    {
        var logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        _logger.LogInformation("Opening log folder at {LogFolderPath}.", logFolderPath);
        Process.Start(new ProcessStartInfo("explorer.exe", logFolderPath) { UseShellExecute = true });
    }

    public void OpenSupportDiscord() => OpenUrl("https://discord.gg/rtGXwMn7pX", "Opening Discord.");

    public void OpenXivArchive() => OpenUrl("https://www.xivmodarchive.com/", "Opening XivArchive.");

    public void OpenHelios() => OpenUrl("https://heliosphere.app/", "Opening Helios.");

    public void OpenDonate() => OpenUrl("https://ko-fi.com/sebastina", "Opening Donate.");
    
    public void OpenGlamourDresser() => OpenUrl("https://www.glamourdresser.com/", "Opening Glamour Dresser.");
    public void OpenNexusMods() => OpenUrl("https://www.nexusmods.com/finalfantasy14", "Opening Nexus Mods.");
    public void OpenAetherLink() => OpenUrl("https://beta.aetherlink.app/", "Opening Aether Link.");
    public void OpenPrettyKitty() => OpenUrl("https://prettykittyemporium.blogspot.com/?zx=67bbd385fd16c2ff", "Opening Pretty Kitty.");
    public void OpenArk() => OpenUrl("https://github.com/Sebane1/RoleplayingVoiceDalamud", "Opening Ark.");
}