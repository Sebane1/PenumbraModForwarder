using System;
using System.IO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services
{
    public sealed class ConfigurationService : IConfigurationService
    {
        private string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\config.json";
        private ConfigurationModel _config;
        private readonly object _lock = new();
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IErrorWindowService _errorWindowService;
        private readonly IMapper _mapper;
        
        public ConfigurationService(ILogger<ConfigurationService> logger, IMapper mapper, IErrorWindowService errorWindowService)
        {
            _logger = logger;
            _mapper = mapper;
            _errorWindowService = errorWindowService;
            if (!File.Exists(_configPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                _config = new ConfigurationModel
                {
                    AdvancedOptions = new AdvancedConfigurationModel()
                };
                SaveConfig();
            }
            else
            {
                LoadConfig();
            }
        }

        // Event to notify subscribers when the configuration changes
        public event EventHandler ConfigChanged;

        public T GetConfigValue<T>(Func<ConfigurationModel, T> getValue)
        {
            _logger.LogDebug("Getting config value");
            lock (_lock)
            {
                return getValue(_config);
            }
        }

        public void SetConfigValue<T>(Action<ConfigurationModel, T> setValue, T value)
        {
            _logger.LogDebug("Setting config value");
            lock (_lock)
            {
                setValue(_config, value);
                SaveConfig();
                OnConfigChanged();
            }
        }

        public bool IsAdvancedOptionEnabled(Func<AdvancedConfigurationModel, bool> advancedOption)
        {
            _logger.LogDebug("Checking advanced config option");
            lock (_lock)
            {
                return advancedOption(_config.AdvancedOptions);
            }
        }

        public void MigrateOldConfig()
        {
            var filesToMigrate = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\DownloadPath.config",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\AutoLoad.config",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\Config.json"
            };

            foreach (var file in filesToMigrate)
            {
                _logger.LogDebug($"Checking for old config file: {file}");
                if (!File.Exists(file))
                {
                    _logger.LogDebug("Old config file not found");
                    continue;
                }

                try
                {
                    var oldConfig = File.ReadAllText(file);

                    if (file.Contains("DownloadPath"))
                    {
                        _logger.LogDebug("Migrating DownloadPath");
                        SetConfigValue((config, value) => config.DownloadPath = value, oldConfig);
                    }
                    else if (file.Contains("AutoLoad"))
                    {
                        _logger.LogDebug("Migrating AutoLoad");
                        SetConfigValue((config, value) => config.AutoLoad = value, bool.Parse(oldConfig));
                    }
                    else if (file.Contains("Config.json"))
                    {
                        _logger.LogDebug("Migrating Config.json");
                        if (string.IsNullOrWhiteSpace(oldConfig) || !oldConfig.StartsWith("{"))
                        {
                            _logger.LogError("Old config file is empty or invalid JSON");
                            return;
                        }

                        var oldConfigModel = JsonConvert.DeserializeObject<OldConfigurationModel>(oldConfig);
                        if (oldConfigModel == null)
                        {
                            _logger.LogError("Failed to deserialize old config model");
                            // If we get this error here, we are in some serious trouble.
                            _errorWindowService.ShowError("Failed to deserialize old config model");
                            return;
                        }

                        var newConfigModel = _mapper.Map<ConfigurationModel>(oldConfigModel);
                        _config = newConfigModel;
                        SaveConfig();
                        _logger.LogInformation("Migrated Config.json");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing file: {file}");
                    _errorWindowService.ShowError(ex.ToString());
                }
            }

            var oldConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                     @"\PenumbraModForwarder\PenumbraModForwarder";
            if (Directory.Exists(oldConfigDirectory))
            {
                _logger.LogDebug("Deleting old config directory");
                Directory.Delete(oldConfigDirectory, true);
            }
        }


        private void SaveConfig()
        {
            _logger.LogDebug("Saving config");
            lock (_lock)
            {
                File.WriteAllText(_configPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
        }

        private void LoadConfig()
        {
            lock (_lock)
            {
                var rawJson = File.ReadAllText(_configPath);

                _config = JsonConvert.DeserializeObject<ConfigurationModel>(rawJson);

                if (_config == null)
                {
                    _logger.LogWarning("Configuration deserialization failed. Creating a new configuration with default values.");
                    _config = new ConfigurationModel();
                }

                if (_config.AdvancedOptions == null)
                {
                    _logger.LogInformation("AdvancedOptions is missing in the configuration file, adding default values.");
                    _config.AdvancedOptions = new AdvancedConfigurationModel();
                }

                PopulateDefaultValues(_config.AdvancedOptions);
                SaveConfig();
            }
        }

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

        private void OnConfigChanged()
        {
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
