using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IConfigurationService
{
    public T GetConfigValue<T>(Func<ConfigurationModel, T> getValue);
    public void SetConfigValue<T>(Action<ConfigurationModel, T> setValue, T value);
}