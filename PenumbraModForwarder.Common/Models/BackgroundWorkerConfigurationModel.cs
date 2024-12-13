using System.ComponentModel.DataAnnotations;

namespace PenumbraModForwarder.Common.Models;

public class BackgroundWorkerConfigurationModel
{
    [Display(Name = "Auto Delete", GroupName = "General")]
    public bool AutoDelete { get; set; } = true;
    [Display(Name = "Extract All", GroupName = "Extraction")]
    public bool ExtractAll { get; set; }
    [Display(Name = "Extract To", GroupName = "Extraction")]
    public string ExtractTo { get; set; } = string.Empty;
    [Display(Name = "Download Path", GroupName = "Pathing")]
    public List<string> DownloadPath { get; set; } = new();
    [Display(Name = "TexTool Path", GroupName = "Pathing")]
    public string TexToolPath { get; set; } = string.Empty;
}