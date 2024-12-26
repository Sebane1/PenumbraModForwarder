using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Attributes;
using PenumbraModForwarder.Common.Helpers;

namespace PenumbraModForwarder.Common.Models;

public class BackgroundWorkerConfigurationModel
{
    [Display(Name = "Auto Delete", GroupName = "General", Description = "Automatically delete files after we are done with them")]
    public bool AutoDelete { get; set; } = true;

    [Display(Name = "Install All", GroupName = "General", Description = "Install every mod inside an archive")]
    public bool InstallAll { get; set; }

    [Display(Name = "Extraction Path", GroupName = "Extraction", Description = "Where to extract archive contents to")]
    public string ExtractTo { get; set; } = Consts.ConfigurationConsts.ExtractionPath;
    
    [ExcludeFromSettingsUI]
    private List<string> _downloadPath = [DefaultDownloadPath.GetDefaultDownloadPath()];
    
    [Display(Name = "Download Path", GroupName = "Pathing", Description = "The path to check for modded files")]
    public List<string> DownloadPath
    {
        get => _downloadPath;
        set => _downloadPath = value.Distinct().ToList();
    }

    [Display(Name = "TexTool Path", GroupName = "Pathing", Description = "The path to Textools")]
    public string TexToolPath { get; set; } = string.Empty;
    [Display(Name = "Skip Endwalker and below mods", GroupName = "General", Description = "Skip endwalker and below mods")]
    public bool SkipPreDt  { get; set; } = true;
    [Display(Name = "Penumbra Mod Folder", GroupName = "Pathing", Description = "Penumbra Mod Folder")]
    public string PenumbraModFolder { get; set; } = string.Empty;
}