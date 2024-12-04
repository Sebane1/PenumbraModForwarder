using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
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
            _config = JsonConvert.DeserializeObject<ConfigurationModel>(configContent)
                      ?? new ConfigurationModel();
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
            // Determine what changed
            var changes = GetChanges(originalConfig, updatedConfig);
            foreach (var change in changes)
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(change.Key, change.Value));
            }
        }
    }

    private void SaveConfiguration()
    {
        var updatedConfigContent = JsonConvert.SerializeObject(_config, Formatting.Indented);
        _fileStorage.Write(ConfigurationConsts.ConfigurationFilePath, updatedConfigContent);
    }

    private Dictionary<string, object> GetChanges(string originalConfig, string updatedConfig)
    {
        var original = JsonConvert.DeserializeObject<ConfigurationModel>(originalConfig);
        var updated = JsonConvert.DeserializeObject<ConfigurationModel>(updatedConfig);

        var changes = new Dictionary<string, object>();
        foreach (var property in typeof(ConfigurationModel).GetProperties())
        {
            var originalValue = property.GetValue(original);
            var updatedValue = property.GetValue(updated);

            if (!Equals(originalValue, updatedValue))
            {
                changes[property.Name] = updatedValue;
            }
        }

        return changes;
    }
}