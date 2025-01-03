using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Attributes;

namespace PenumbraModForwarder.Common.Models;

public class BackgroundWorkerConfigurationModel
{
    [Display(Name = "Auto Delete Files", GroupName = "General", Description = "Automatically delete files after we are done with them")]
    public bool AutoDelete { get; set; } = true;

    [Display(Name = "Install All Mods", GroupName = "General", Description = "Install every mod inside an archive")]
    public bool InstallAll { get; set; }

    [Display(Name = "Mod Folder Path", GroupName = "Pathing", Description = "Where to move the mods to for processing")]
    public string ModFolderPath { get; set; } = Consts.ConfigurationConsts.ModsPath;

    [ExcludeFromSettingsUI] private List<string> _downloadPath = [];
    
    [Display(Name = "Download Path", GroupName = "Pathing", Description = "The path to check for modded files")]
    public List<string> DownloadPath
    {
        get => _downloadPath;
        set => _downloadPath = value.Distinct().ToList();
    }

    [Display(Name = "TexTool ConsoleTools.exe Path", GroupName = "Pathing", Description = "The path to Textool's Console Tools.exe")]
    public string TexToolPath { get; set; } = string.Empty;
    [Display(Name = "Skip Endwalker and below mods", GroupName = "General", Description = "Skip endwalker and below mods")]
    public bool SkipPreDt  { get; set; } = true;
    [Display(Name = "Penumbra Mod Folder Path", GroupName = "Pathing", Description = "Penumbra Mod Folder")]
    public string PenumbraModFolderPath { get; set; } = string.Empty;
}