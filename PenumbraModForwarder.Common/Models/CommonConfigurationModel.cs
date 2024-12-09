namespace PenumbraModForwarder.Common.Models;

public class CommonConfigurationModel
{
    public bool AutoLoad { get; set; }
    public bool AutoDelete { get; set; }
    public bool ExtractAll { get; set; }
    public bool NotificationEnabled { get; set; }
    public bool FileLinkingEnabled { get; set; }
    public bool StartOnBoot { get; set; }
    
    public List<string> DownloadPath { get; set; } = new();
    public string TexToolPath { get; set; } = string.Empty;
    public string GitHubOwner { get; set; } = "Sebane1";
    public string GitHubRepo { get; set; } = "PenumbraModForwarder";
    public bool IncludePrereleases { get; set; }
}