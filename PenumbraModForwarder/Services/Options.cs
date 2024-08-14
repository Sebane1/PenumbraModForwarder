using Newtonsoft.Json;
using FFXIVModExractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVModExractor.Services
{
    internal static class Options
    {
        private static string _configPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
        public static void WriteToConfig(Config config)
        {
            CreateConfigFile();

            string configFilePath = Path.Combine(_configPath, "Config.json");
            string json = File.ReadAllText(configFilePath);
            List<Config> existingConfigs = JsonConvert.DeserializeObject<List<Config>>(json) ?? new List<Config>();

            Config existingConfig = existingConfigs.FirstOrDefault(c => c.Option == config.Option);
            if (existingConfig != null)
            {
                existingConfig.Value = config.Value;
            }
            else
            {
                existingConfigs.Add(config);
            }

            string updatedJson = JsonConvert.SerializeObject(existingConfigs, Formatting.Indented);
            File.WriteAllText(configFilePath, updatedJson);
        }

        public static Config ReadFromConfig(string option)
        {
            CreateConfigFile();

            string configFilePath = Path.Combine(_configPath, "Config.json");
            if (!File.Exists(configFilePath))
            {
                return new Config();
            }

            string json = File.ReadAllText(configFilePath);
            List<Config> existingConfigs = JsonConvert.DeserializeObject<List<Config>>(json) ?? new List<Config>();

            Config existingConfig = existingConfigs.FirstOrDefault(c => c.Option == option);
            if (existingConfig != null)
            {
                return existingConfig;
            }
            else
            {
                return new Config();
            }
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
    }
}
