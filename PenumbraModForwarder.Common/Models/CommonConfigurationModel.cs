using System.ComponentModel.DataAnnotations;
using PenumbraModForwarder.Common.Attributes;

namespace PenumbraModForwarder.Common.Models;

public class CommonConfigurationModel
{
    [Display(Name = "Auto Load", GroupName = "General")]
    public bool AutoLoad { get; set; }
    [Display(Name = "File Linking", GroupName = "General")]
    public bool FileLinkingEnabled { get; set; }
    [Display(Name = "Start on Boot", GroupName = "General")]
    public bool StartOnBoot { get; set; }
    [Display(Name = "Enable Beta Builds", GroupName = "Updates")]
    public bool IncludePrereleases { get; set; }
}