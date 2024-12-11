using System.ComponentModel.DataAnnotations;

namespace PenumbraModForwarder.Common.Models;

public class AdvancedConfigurationModel
{
    [Display(Name = "Hide Window On Startup", GroupName = "Mod Forwarder")]
    public bool HideWindowOnStartup { get; set; } = true;
    [Display(Name = "Penumbra Api Timeout (Seconds)", GroupName = "Penumbra")]
    public int PenumbraTimeOutInSeconds { get; set; } = 60;
    [Display(Name = "Show Watchdog Window", GroupName = "Mod Forwarder")]
    public bool ShowWatchDogWindow { get; set; } = false;
}