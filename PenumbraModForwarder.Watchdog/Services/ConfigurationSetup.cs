using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Watchdog.Interfaces;

namespace PenumbraModForwarder.Watchdog.Services
{
    public class ConfigurationSetup : IConfigurationSetup
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IConfigurationService _configurationService;

        public ConfigurationSetup(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public void CreateFiles()
        {
            _logger.Info("Creating configuration files");

            _configurationService.CreateConfiguration();
        }
    }
}