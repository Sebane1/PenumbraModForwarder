using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services;

public class ConfigurationService : IConfigurationService
{
    /// <summary>
    /// Notify anything listening that the configuration has been updated
    /// </summary>
    public event EventHandler ConfigurationChanged;
    
    /// <summary>
    /// Create a config file with the default values
    /// </summary>
    public void CreateConfiguration()
    {
        if (!Directory.Exists(ConfigurationConsts.ConfigurationPath)) Directory.CreateDirectory(ConfigurationConsts.ConfigurationPath);
        
        if (!Directory.Exists(ConfigurationConsts.ConversionPath)) Directory.CreateDirectory(ConfigurationConsts.ConversionPath);
        
        if (!Directory.Exists(ConfigurationConsts.ExtractionPath)) Directory.CreateDirectory(ConfigurationConsts.ExtractionPath);
        
        if (!Directory.Exists(ConfigurationConsts.ModsPath)) Directory.CreateDirectory(ConfigurationConsts.ModsPath);

        if (!File.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            File.WriteAllText(ConfigurationConsts.ConfigurationFilePath, JsonConvert.SerializeObject(new ConfigurationModel(), Formatting.Indented));
        }
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
            throw new ArgumentNullException(nameof(propertySelector), "Property selector cannot be null.");
        }

        if (!File.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            throw new FileNotFoundException("Configuration file not found.", ConfigurationConsts.ConfigurationFilePath);
        }

        var configContent = File.ReadAllText(ConfigurationConsts.ConfigurationFilePath);
        var configModel = JsonConvert.DeserializeObject<ConfigurationModel>(configContent);

        if (configModel == null)
        {
            throw new InvalidOperationException("Failed to deserialize the configuration file.");
        }

        return propertySelector(configModel);
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

        if (!File.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            throw new FileNotFoundException("Configuration file not found.", ConfigurationConsts.ConfigurationFilePath);
        }

        var configContent = File.ReadAllText(ConfigurationConsts.ConfigurationFilePath);
        var configModel = JsonConvert.DeserializeObject<ConfigurationModel>(configContent);

        if (configModel == null)
        {
            throw new InvalidOperationException("Failed to deserialize the configuration file.");
        }

        propertyUpdater(configModel);

        var updatedConfigContent = JsonConvert.SerializeObject(configModel, Formatting.Indented);
        File.WriteAllText(ConfigurationConsts.ConfigurationFilePath, updatedConfigContent);

        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
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