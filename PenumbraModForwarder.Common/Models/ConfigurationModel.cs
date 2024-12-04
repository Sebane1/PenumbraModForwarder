namespace PenumbraModForwarder.Common.Models;

// TODO: We need to split this up into to separate models
// 1 for UI App settings
// 1 for Background Worker Settings
// 1 for General Settings
public class ConfigurationModel
{
    public bool AutoLoad { get; set; }
    public bool AutoDelete { get; set; }
    public bool ExtractAll { get; set; }
    public bool NotificationEnabled { get; set; }
    public bool FileLinkingEnabled { get; set; }
    public bool StartOnBoot { get; set; }
    public List<string> DownloadPath { get; set; } = [];
    public string TexToolPath { get; set; } = string.Empty;
    public AdvancedConfigurationModel AdvancedOptions { get; set; } = new();
}