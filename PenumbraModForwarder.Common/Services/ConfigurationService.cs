using System;
using System.IO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services
{
    public class ConfigurationService : IConfigurationService
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
            MigrateOldConfig();
            if (!File.Exists(_configPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                _config = new ConfigurationModel();
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
            _logger.LogInformation("Getting config value");
            lock (_lock)
            {
                return getValue(_config);
            }
        }

        public void SetConfigValue<T>(Action<ConfigurationModel, T> setValue, T value)
        {
            _logger.LogInformation("Setting config value");
            lock (_lock)
            {
                setValue(_config, value);
                SaveConfig();
                OnConfigChanged();
            }
        }

        private void MigrateOldConfig()
        {
            var filesToMigrate = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\DownloadPath.config",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\AutoLoad.config",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\PenumbraModForwarder\Config.json"
            };

            foreach (var file in filesToMigrate)
            {
                _logger.LogInformation($"Checking for old config file: {file}");
                if (!File.Exists(file))
                {
                    _logger.LogInformation("Old config file not found");
                    continue;
                }

                try
                {
                    var oldConfig = File.ReadAllText(file);

                    if (file.Contains("DownloadPath"))
                    {
                        _logger.LogInformation("Migrating DownloadPath");
                        SetConfigValue((config, value) => config.DownloadPath = value, oldConfig);
                    }
                    else if (file.Contains("AutoLoad"))
                    {
                        _logger.LogInformation("Migrating AutoLoad");
                        SetConfigValue((config, value) => config.AutoLoad = value, bool.Parse(oldConfig));
                    }
                    else if (file.Contains("Config.json"))
                    {
                        _logger.LogInformation("Migrating Config.json");
                        if (string.IsNullOrWhiteSpace(oldConfig) || !oldConfig.StartsWith("{"))
                        {
                            _logger.LogInformation("Old config file is empty or invalid JSON");
                            continue;
                        }

                        var oldConfigModel = JsonConvert.DeserializeObject<OldConfigurationModel>(oldConfig);
                        if (oldConfigModel == null)
                        {
                            _logger.LogWarning("Failed to deserialize old config model");
                            continue;
                        }

                        var newConfigModel = _mapper.Map<ConfigurationModel>(oldConfigModel);
                        _config = newConfigModel;
                        SaveConfig();
                        _logger.LogInformation("Migrated Config.json");
                    }

                    // Uncomment if you want to delete the old config files
                    // _logger.LogInformation("Deleting old config file");
                    // File.Delete(file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing file: {file}");
                    _errorWindowService.ShowError(ex.ToString());
                }
            }

            // Uncomment if you want to delete the old config directory
            // var oldConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            //                          @"\PenumbraModForwarder\PenumbraModForwarder";
            // if (Directory.Exists(oldConfigDirectory))
            // {
            //     _logger.LogInformation("Deleting old config directory");
            //     Directory.Delete(oldConfigDirectory, true);
            // }
        }


        private void SaveConfig()
        {
            _logger.LogInformation("Saving config");
            lock (_lock)
            {
                File.WriteAllText(_configPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
        }

        private void LoadConfig()
        {
            lock (_lock)
            {
                _config = JsonConvert.DeserializeObject<ConfigurationModel>(File.ReadAllText(_configPath));
            }
        }

        protected virtual void OnConfigChanged()
        {
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
