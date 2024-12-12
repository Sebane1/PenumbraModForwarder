using System.ComponentModel.DataAnnotations;

namespace PenumbraModForwarder.Common.Models;

public class WatchdogConfigurationModel
{
    [Display(Name = "Auto Delete", GroupName = "General")]
    public bool AutoDelete { get; set; } = true;
    [Display(Name = "Extract All", GroupName = "General")]
    public bool ExtractAll { get; set; }
    [Display(Name = "Download Path", GroupName = "Pathing")]
    public List<string> DownloadPath { get; set; } = new();
    [Display(Name = "TexTool Path", GroupName = "Pathing")]
    public string TexToolPath { get; set; } = string.Empty;
}