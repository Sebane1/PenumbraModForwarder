using Newtonsoft.Json;
using FFXIVModExractor.Models;
using System;
using System.IO;

namespace FFXIVModExractor.Services
{
    internal static class Options
    {
        private static string _configPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
        private static Config _cachedConfig;
        private static readonly object _lock = new object();

        public static void UpdateConfig(Action<Config> action)
        {
            var config = GetConfig();
            action(config);
            WriteToConfig(config);
        }

        /// <summary>
        /// Retrieves a configuration value from a Config object based on the provided property name.
        /// </summary>
        /// <typeparam name="T">The type to which the property value should be cast.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve from the Config object.</param>
        /// <returns>The value of the specified property cast to type T.</returns>
        /// <exception cref="ArgumentException">Thrown when the property with the given name does not exist on the Config object.</exception>
        /// <exception cref="InvalidCastException">Thrown when the value of the property cannot be cast to type T.</exception>
        public static T GetConfigValue<T>(string propertyName)
        {
            var config = GetConfig();

            var property = typeof(Config).GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on Config.");
            }

            var value = property.GetValue(config);
            if (value is T result)
            {
                return result;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast property '{propertyName}' to type {typeof(T)}.");
            }
        }

        private static Config GetConfig()
        {
            if (_cachedConfig == null)
            {
                lock (_lock)
                {
                    if (_cachedConfig == null)
                    {
                        _cachedConfig = ReadFromConfig();
                    }
                }
            }
            return _cachedConfig;
        }

        private static void WriteToConfig(Config config)
        {
            CreateConfigFile();

            string configFilePath = Path.Combine(_configPath, "Config.json");

            string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(configFilePath, updatedJson);
            _cachedConfig = config;
        }

        private static Config ReadFromConfig()
        {
            CreateConfigFile();

            string configFilePath = Path.Combine(_configPath, "Config.json");
            if (!File.Exists(configFilePath))
            {
                return new Config();
            }

            string json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
        }

        private static void CreateConfigFile()
        {
            try
            {
                string configFilePath = Path.Combine(_configPath, "Config.json");
                if (!File.Exists(configFilePath))
                {
                    File.Create(configFilePath).Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void MigrateOldSettings()
        {
            var filesToCheck = new List<string>
            {
                "AutoLoad.config",
                "DownloadPath.config"
            };

            foreach (var file in filesToCheck)
            {
                var oldConfig = Path.Combine(_configPath, file);
                if (File.Exists(oldConfig))
                {
                    Console.WriteLine($"{file} exists migrating");

                    var text = File.ReadAllText(oldConfig).Trim(); // Trim to remove any extraneous whitespace

                    if (file.Contains("AutoLoad"))
                    {
                        UpdateConfig(config =>
                        {
                            config.AutoLoad = bool.Parse(text);
                        });
                    }
                    else if (file.Contains("DownloadPath"))
                    {
                        UpdateConfig(config =>
                        {
                            config.DownloadPath = text;
                        });
                    }

                    // Once we are done, let's just delete the old stuff as it's no longer needed
                    File.Delete(oldConfig);
                }
            }
        }
    }
}
