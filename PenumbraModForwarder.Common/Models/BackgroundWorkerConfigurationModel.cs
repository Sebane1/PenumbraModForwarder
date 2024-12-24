using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Helpers;

namespace PenumbraModForwarder.Common.Models;

public class BackgroundWorkerConfigurationModel
{
    [Display(Name = "Auto Delete", GroupName = "General")]
    public bool AutoDelete { get; set; } = true;

    [Display(Name = "Install All", GroupName = "General")]
    public bool InstallAll { get; set; }

    [Display(Name = "Extraction Path", GroupName = "Extraction")]
    public string ExtractTo { get; set; } = Consts.ConfigurationConsts.ExtractionPath;

    [Display(Name = "Download Path", GroupName = "Pathing")]
    public List<string> DownloadPath { get; set; } = new() { DefaultDownloadPath.GetDefaultDownloadPath() };

    [Display(Name = "TexTool Path", GroupName = "Pathing")]
    public string TexToolPath { get; set; } = string.Empty;
    [Display(Name = "Skip Endwalker and below mods", GroupName = "General")]
    public bool SkipPreDt  { get; set; } = true;
}