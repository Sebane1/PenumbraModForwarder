using System.Collections;
using System.Reflection;
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
            SaveConfiguration(_config);
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

    public void SaveConfiguration(ConfigurationModel updatedConfig, bool detectChangesAndInvokeEvents = true)
    {
        if (updatedConfig == null) throw new ArgumentNullException(nameof(updatedConfig));

        if (detectChangesAndInvokeEvents)
        {
            var originalConfig = _config;
            var changes = GetChanges(originalConfig, updatedConfig);
            foreach (var change in changes)
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(change.Key, change.Value));
            }
        }

        _config = updatedConfig;

        var updatedConfigContent = JsonConvert.SerializeObject(_config, Formatting.Indented);
        _fileStorage.Write(ConfigurationConsts.ConfigurationFilePath, updatedConfigContent);
    }

    public void ResetToDefaultConfiguration()
    {
        _config = new ConfigurationModel();
        SaveConfiguration(_config);
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

        // Make a deep copy of the original configuration
        var originalConfig = _config.DeepClone();

        // Log the state of originalConfig
        Log.Debug("Original Config after cloning: {Config}", JsonConvert.SerializeObject(originalConfig));

        // Apply updates
        propertyUpdater(_config);

        // Log the state of updated _config
        Log.Debug("Updated Config after applying propertyUpdater: {Config}", JsonConvert.SerializeObject(_config));

        // Detect changes
        var changes = GetChanges(originalConfig, _config);
        if (changes.Any())
        {
            foreach (var change in changes)
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(change.Key, change.Value));
            }
        }

        // Save configuration without detecting changes again
        SaveConfiguration(_config, detectChangesAndInvokeEvents: false);
    }

    private Dictionary<string, object> GetChanges(ConfigurationModel original, ConfigurationModel updated)
    {
        var changes = new Dictionary<string, object>();
        CompareProperties(original, updated, changes, "");
        return changes;
    }

    private void CompareProperties(object original, object updated, Dictionary<string, object> changes, string parentProperty)
    {
        if (original == null && updated == null)
        {
            return;
        }

        var type = (original ?? updated).GetType();

        // Handle simple types and strings
        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
        {
            if (!Equals(original, updated))
            {
                var propName = string.IsNullOrEmpty(parentProperty) ? type.Name : parentProperty;
                changes[propName] = updated;
            }
            return;
        }

        // Handle collections
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            if (!CompareEnumerables(original as IEnumerable, updated as IEnumerable))
            {
                var propName = string.IsNullOrEmpty(parentProperty) ? type.Name : parentProperty;
                changes[propName] = updated;
            }
            return;
        }

        // Compare properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead) continue;

            var originalValue = original != null ? property.GetValue(original) : null;
            var updatedValue = updated != null ? property.GetValue(updated) : null;

            var newParent = string.IsNullOrEmpty(parentProperty) ? property.Name : $"{parentProperty}.{property.Name}";

            CompareProperties(originalValue, updatedValue, changes, newParent);
        }
    }

    private bool CompareEnumerables(IEnumerable original, IEnumerable updated)
    {
        if (original == null && updated == null) return true;
        if (original == null || updated == null) return false;

        var originalEnum = original.Cast<object>().ToList();
        var updatedEnum = updated.Cast<object>().ToList();

        if (originalEnum.Count != updatedEnum.Count) return false;

        for (int i = 0; i < originalEnum.Count; i++)
        {
            var originalItem = originalEnum[i];
            var updatedItem = updatedEnum[i];

            if (originalItem == null && updatedItem == null) continue;
            if (originalItem == null || updatedItem == null) return false;

            var itemType = originalItem.GetType();

            if (itemType.IsClass && itemType != typeof(string))
            {
                var changes = new Dictionary<string, object>();
                CompareProperties(originalItem, updatedItem, changes, "");
                if (changes.Any()) return false;
            }
            else if (!Equals(originalItem, updatedItem))
            {
                return false;
            }
        }
        return true;
    }
}

// Extension method for deep cloning
public static class CloneExtensions
{
    public static T DeepClone<T>(this T obj)
    {
        if (obj == null) return default(T);

        // Configure serialization settings to include all properties
        var settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto
        };

        var serialized = JsonConvert.SerializeObject(obj, settings);
        return JsonConvert.DeserializeObject<T>(serialized, settings);
    }
}