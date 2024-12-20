using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IConfigurationService
{
    void CreateConfiguration();
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
    void UpdateConfigValue(Action<ConfigurationModel> propertyUpdater, string changedPropertyPath, object newValue);
    object ReturnConfigValue(Func<ConfigurationModel, object> propertySelector);
    void ResetToDefaultConfiguration();
    ConfigurationModel GetConfiguration();
    void SaveConfiguration(ConfigurationModel updatedConfig, bool detectChangesAndInvokeEvents = true);
    void UpdateConfigFromExternal(string propertyPath, object newValue);
}