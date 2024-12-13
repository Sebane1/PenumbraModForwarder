namespace PenumbraModForwarder.Common.Models;

public class ConfigurationModel
{
    public UIConfigurationModel UI { get; set; } = new();
    public BackgroundWorkerConfigurationModel BackgroundWorker { get; set; } = new();
    public CommonConfigurationModel Common { get; set; } = new();
    public AdvancedConfigurationModel AdvancedOptions { get; set; } = new();
}