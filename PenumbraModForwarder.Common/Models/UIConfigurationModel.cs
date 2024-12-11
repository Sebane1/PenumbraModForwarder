using System.ComponentModel.DataAnnotations;

namespace PenumbraModForwarder.Common.Models;

public class UIConfigurationModel
{
    [Display(Name = "Notification Enabled", GroupName = "Notification")]
    public bool NotificationEnabled { get; set; } = true;

    [Display(Name = "Notification Sound", GroupName = "Notification")]
    public bool NotificationSoundEnabled { get; set; } 
}