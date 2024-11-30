using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IFileStorage _fileStorage;
    private ConfigurationModel _config;
    
    /// <summary>
    /// Notify anything listening that the configuration has been updated
    /// </summary>
    public event EventHandler ConfigurationChanged;

    public ConfigurationService(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
        LoadConfiguration();
    }

    /// <summary>
    /// Load the in memory file system
    /// </summary>
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
    
    /// <summary>
    /// Create a config file with the default values
    /// </summary>
    public void CreateConfiguration()
    {
        _fileStorage.CreateDirectory(ConfigurationConsts.ConfigurationPath);
        _fileStorage.CreateDirectory(ConfigurationConsts.ConversionPath);
        _fileStorage.CreateDirectory(ConfigurationConsts.ExtractionPath);
        _fileStorage.CreateDirectory(ConfigurationConsts.ModsPath);

        if (!_fileStorage.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            SaveConfiguration();
        }
    }
    
    /// <summary>
    /// Reset the config file to default
    /// </summary>
    public void ResetToDefaultConfiguration()
    {
        _config = new ConfigurationModel();
        SaveConfiguration();
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Get a specific property value from the configuration model
    /// </summary>
    /// <param name="propertySelector">A function to select the property from the ConfigurationModel.</param>
    /// <returns>The value of the specified property.</returns>
    public object ReturnConfigValue(Func<ConfigurationModel, object> propertySelector)
    {
        if (propertySelector == null)
        {
            throw new ArgumentNullException(nameof(propertySelector));
        }

        return propertySelector(_config);
    }
    
    /// <summary>
    /// Update a specific property value in the configuration model and save it to the file
    /// </summary>
    /// <param name="propertyUpdater">A function to update the property in the ConfigurationModel.</param>
    public void UpdateConfigValue(Action<ConfigurationModel> propertyUpdater)
    {
        if (propertyUpdater == null)
        {
            throw new ArgumentNullException(nameof(propertyUpdater), "Property updater cannot be null.");
        }

        propertyUpdater(_config);
        SaveConfiguration();

        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Save the configuration to a file
    /// </summary>
    private void SaveConfiguration()
    {
        var updatedConfigContent = JsonConvert.SerializeObject(_config, Formatting.Indented);
        _fileStorage.Write(ConfigurationConsts.ConfigurationFilePath, updatedConfigContent);
    }
    
    /// <summary>
    /// Populate the default values
    /// This will probably only be used when a new option is added to configuration
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    private void PopulateDefaultValues<T>(T obj) where T : class, new()
    {
        var defaultInstance = new T();

        var properties = typeof(T).GetProperties();
    
        foreach (var property in properties)
        {
            var currentValue = property.GetValue(obj);
            var defaultValue = property.GetValue(defaultInstance);

            // If current value is null or default (like 0 for int), replace it with the default value.
            if (currentValue == null || (property.PropertyType.IsValueType && currentValue.Equals(Activator.CreateInstance(property.PropertyType))))
            {
                property.SetValue(obj, defaultValue);
            }
        }
    }
}