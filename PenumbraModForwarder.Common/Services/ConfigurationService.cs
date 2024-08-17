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
                    _config = JsonConvert.DeserializeObject<ConfigurationModel>(oldConfig);
                    SaveConfig();
                }
                
                _logger.LogInformation("Deleting old config file");
                File.Delete(file);
            }

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                  @"\PenumbraModForwarder\PenumbraModForwarder")) return;
            _logger.LogInformation("Deleting old config directory");
            Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                             @"\PenumbraModForwarder\PenumbraModForwarder", true);
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
        
        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
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
    }
}
