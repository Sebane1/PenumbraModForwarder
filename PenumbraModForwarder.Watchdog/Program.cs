using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Watchdog.Extensions;
using PenumbraModForwarder.Watchdog.Imports;
using PenumbraModForwarder.Watchdog.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Watchdog
{
    class Program
    {
        private readonly ILogger _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IProcessManager _processManager;
        private readonly IConfigurationSetup _configurationSetup;
        private readonly IUpdateService _updateService; 

        public Program(
            IConfigurationService configurationService,
            IProcessManager processManager,
            IConfigurationSetup configurationSetup, IUpdateService updateService)
        {
            _configurationService = configurationService;
            _processManager = processManager;
            _configurationSetup = configurationSetup;
            _updateService = updateService;
            _logger = Log.ForContext<Program>();
        }

        static void Main(string[] args)
        {
            bool isNewInstance;
            using (new Mutex(true, "PenumbraModForwarder.Launcher", out isNewInstance))
            {
                if (!isNewInstance)
                {
                    Console.WriteLine("Another instance is already running. Exiting...");
                    return;
                }

                var services = new ServiceCollection();
                services.AddApplicationServices();
                services.AddSingleton<Program>();

                var serviceProvider = services.BuildServiceProvider();

                var program = serviceProvider.GetRequiredService<Program>();
                program.Run(args);
            }
        }

        public void Run(string[] args)
        {
            _configurationSetup.CreateFiles();

            // Set initialization flag before starting processes
            ApplicationBootstrapper.SetWatchdogInitialization();

            // Set the environment variable for child processes
            Environment.SetEnvironmentVariable("WATCHDOG_INITIALIZED", "true");

            // Hide the console window on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                HideConsoleWindow();
            }
            
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var semVersion = version == null 
                ? "Local Build" 
                : $"{version.Major}.{version.Minor}.{version.Build}";

            if (_updateService.NeedsUpdateAsync(semVersion).GetAwaiter().GetResult())
            {
                // Run the Updater
                _logger.Information("Update detected, launching updater");
                Environment.Exit(0);
            }

            _processManager.Run();
        }

        void HideConsoleWindow()
        {
            var showWindow =
                (bool)_configurationService.ReturnConfigValue(config => config.AdvancedOptions.ShowWatchDogWindow);

            if (showWindow)
            {
                _logger.Information("Showing watchdog window");
                return;
            }

            var handle = DllImports.GetConsoleWindow();
            if (handle == IntPtr.Zero)
                return;

            _logger.Information("Hiding console window");
            DllImports.ShowWindow(handle, DllImports.SW_HIDE);
            _logger.Information("Console window should now be hidden.");
        }
    }
}