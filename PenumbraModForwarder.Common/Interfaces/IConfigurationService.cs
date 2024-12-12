using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IConfigurationService
{
    void CreateConfiguration();
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
    void UpdateConfigValue(Action<ConfigurationModel> propertyUpdater);
    object ReturnConfigValue(Func<ConfigurationModel, object> propertySelector);
    void ResetToDefaultConfiguration();
    ConfigurationModel GetConfiguration();
}