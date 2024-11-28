namespace PenumbraModForwarder.Common.Interfaces;

public interface IConfigurationService
{
    public void CreateConfiguration();
    public event EventHandler ConfigurationChanged;
}