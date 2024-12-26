using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Attributes;
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
    
    [ExcludeFromSettingsUI]
    private List<string> _downloadPath = [DefaultDownloadPath.GetDefaultDownloadPath()];
    
    [Display(Name = "Download Path", GroupName = "Pathing")]
    public List<string> DownloadPath
    {
        get => _downloadPath;
        set => _downloadPath = value.Distinct().ToList();
    }

    [Display(Name = "TexTool Path", GroupName = "Pathing")]
    public string TexToolPath { get; set; } = string.Empty;
    [Display(Name = "Skip Endwalker and below mods", GroupName = "General")]
    public bool SkipPreDt  { get; set; } = true;
    [Display(Name = "Penumbra Mod Folder", GroupName = "Pathing")]
    public string PenumbraModFolder { get; set; } = string.Empty;
}