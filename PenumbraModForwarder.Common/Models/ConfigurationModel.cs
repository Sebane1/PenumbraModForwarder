namespace PenumbraModForwarder.Common.Models;

public class ConfigurationModel
{
    public UIConfigurationModel UI { get; set; } = new();
    public WatchdogConfigurationModel Watchdog { get; set; } = new();
    public CommonConfigurationModel Common { get; set; } = new();
    public AdvancedConfigurationModel AdvancedOptions { get; set; } = new();
}