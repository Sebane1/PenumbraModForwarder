using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services;

public class ConfigurationService : IConfigurationService
{
    public event EventHandler ConfigurationChanged;


    public void CreateConfiguration()
    {
        if (!Directory.Exists(ConfigurationConsts.ConfigurationPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationConsts.ConfigurationPath));
        }

        if (!File.Exists(ConfigurationConsts.ConfigurationFilePath))
        {
            File.WriteAllText(ConfigurationConsts.ConfigurationFilePath, JsonConvert.SerializeObject(new ConfigurationModel(), Formatting.Indented));
        }
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