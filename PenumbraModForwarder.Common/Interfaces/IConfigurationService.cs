using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IConfigurationService
{
    public void CreateConfiguration();
    public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
    public void UpdateConfigValue(Action<ConfigurationModel> propertyUpdater);
    public object ReturnConfigValue(Func<ConfigurationModel, object> propertySelector);
    public void ResetToDefaultConfiguration();
}