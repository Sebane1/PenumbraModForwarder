using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Watchdog.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Watchdog.Services
{
    public class ConfigurationSetup : IConfigurationSetup
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;

        public ConfigurationSetup(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _logger = Log.ForContext<ConfigurationSetup>();
        }

        public void CreateFiles()
        {
            _logger.Information("Creating configuration files");
            _configurationService.CreateConfiguration();
        }
    }
}