using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Attributes;

namespace PenumbraModForwarder.Common.Models;

public class CommonConfigurationModel
{
    [Display(Name = "Auto Load", GroupName = "General", Description = "????")]
    public bool AutoLoad { get; set; }
    [Display(Name = "File Linking", GroupName = "General", Description = "Link double clicking files to running in PMF")]
    public bool FileLinkingEnabled { get; set; }
    [Display(Name = "Start on Boot", GroupName = "General", Description = "Start on Computer Boot")]
    public bool StartOnBoot { get; set; }
    [Display(Name = "Enable Beta Builds", GroupName = "Updates", Description = "Enable Beta Builds")]
    public bool IncludePrereleases { get; set; }
    [Display(Name = "Start on FFXIV Boot", GroupName = "General", Description = "Put PMF into xivLauncher's config to make it run")]
    public bool StartOnFfxivBoot { get; set; }
}