namespace PenumbraModForwarder.Common.Models
{
    public class ConfigurationModel
    {
        public bool AutoLoad { get; set; }
        public bool AutoDelete { get; set; }
        public bool ExtractAll { get; set; }
        public bool NotificationEnabled { get; set; }
        public string DownloadPath { get; set; } = string.Empty;
        public string TexToolPath { get; set; } = string.Empty;
    }
}
