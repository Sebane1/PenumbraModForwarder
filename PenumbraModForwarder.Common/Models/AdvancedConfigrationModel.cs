namespace PenumbraModForwarder.Common.Models
{
    public class AdvancedConfigurationModel
    {
        public bool HideWindowOnStartup { get; set; } = true;
        public int PenumbraTimeOut { get; set; } = 60; // This is in Seconds
    }
}

