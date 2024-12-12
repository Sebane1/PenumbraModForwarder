using System.Collections;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IFileStorage _fileStorage;
    private ConfigurationModel _config;

    public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

    public ConfigurationService(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        if (_fileStorage.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            var configContent = _fileStorage.Read(ConfigurationConsts.ConfigurationFilePath);
            _config = JsonConvert.DeserializeObject<ConfigurationModel>(configContent) ?? new ConfigurationModel();
        }
        else
        {
            _config = new ConfigurationModel();
        }
    }

    public void CreateConfiguration()
    {
        var configDirectory = Path.GetDirectoryName(ConfigurationConsts.ConfigurationFilePath);
        if (!_fileStorage.Exists(configDirectory))
        {
            _fileStorage.CreateDirectory(configDirectory);
        }

        if (!_fileStorage.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            _config = new ConfigurationModel();
            SaveConfiguration();
            Log.Information("Configuration file created with default values.");
        }
        else
        {
            Log.Information("Configuration file already exists.");
        }
    }

    public ConfigurationModel GetConfiguration()
    {
        return _config;
    }

    private void SaveConfiguration()
    {
        var updatedConfigContent = JsonConvert.SerializeObject(_config, Formatting.Indented);
        _fileStorage.Write(ConfigurationConsts.ConfigurationFilePath, updatedConfigContent);
    }

    public void ResetToDefaultConfiguration()
    {
        _config = new ConfigurationModel();
        SaveConfiguration();
        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs("All", _config));
    }

    public object ReturnConfigValue(Func<ConfigurationModel, object> propertySelector)
    {
        if (propertySelector == null)
        {
            throw new ArgumentNullException(nameof(propertySelector));
        }
        return propertySelector(_config);
    }

    public void UpdateConfigValue(Action<ConfigurationModel> propertyUpdater)
    {
        if (propertyUpdater == null)
        {
            throw new ArgumentNullException(nameof(propertyUpdater), "Property updater cannot be null.");
        }

        var originalConfig = JsonConvert.SerializeObject(_config);
        propertyUpdater(_config);
        SaveConfiguration();

        var updatedConfig = JsonConvert.SerializeObject(_config);
        if (originalConfig != updatedConfig)
        {
            var changes = GetChanges(originalConfig, updatedConfig);
            foreach (var change in changes)
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(change.Key, change.Value));
            }
        }
    }

    private Dictionary<string, object> GetChanges(string originalConfig, string updatedConfig)
    {
        var original = JsonConvert.DeserializeObject<ConfigurationModel>(originalConfig);
        var updated = JsonConvert.DeserializeObject<ConfigurationModel>(updatedConfig);

        var changes = new Dictionary<string, object>();
        CompareProperties(original, updated, changes, "");
        return changes;
    }

    private void CompareProperties(object original, object updated, Dictionary<string, object> changes, string parentProperty)
    {
        if (original == null || updated == null) return;

        var type = original.GetType();

        // If the type is a collection, compare the contents
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            if (!CompareEnumerables(original as IEnumerable, updated as IEnumerable))
            {
                var propName = string.IsNullOrEmpty(parentProperty) ? type.Name : parentProperty;
                changes[propName] = updated;
            }
            return;
        }

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            // Skip indexer properties
            if (property.GetIndexParameters().Length > 0)
                continue;

            var originalValue = property.GetValue(original);
            var updatedValue = property.GetValue(updated);

            var propertyType = property.PropertyType;

            var newParent = string.IsNullOrEmpty(parentProperty) ? property.Name : $"{parentProperty}.{property.Name}";

            if (propertyType.IsClass && propertyType != typeof(string))
            {
                CompareProperties(originalValue, updatedValue, changes, newParent);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
            {
                if (!CompareEnumerables(originalValue as IEnumerable, updatedValue as IEnumerable))
                {
                    changes[newParent] = updatedValue;
                }
            }
            else if (!Equals(originalValue, updatedValue))
            {
                changes[newParent] = updatedValue;
            }
        }
    }

    private bool CompareEnumerables(IEnumerable original, IEnumerable updated)
    {
        if (original == null && updated == null) return true;
        if (original == null || updated == null) return false;

        var originalEnum = original.Cast<object>().ToList();
        var updatedEnum = updated.Cast<object>().ToList();

        if (originalEnum.Count != updatedEnum.Count)
            return false;

        for (int i = 0; i < originalEnum.Count; i++)
        {
            var originalItem = originalEnum[i];
            var updatedItem = updatedEnum[i];

            if (originalItem == null && updatedItem == null)
                continue;

            if (originalItem == null || updatedItem == null)
                return false;

            if (originalItem.GetType().IsClass && originalItem.GetType() != typeof(string))
            {
                var changes = new Dictionary<string, object>();
                CompareProperties(originalItem, updatedItem, changes, "");
                if (changes.Any())
                    return false;
            }
            else if (!originalItem.Equals(updatedItem))
            {
                return false;
            }
        }
        return true;
    }
}