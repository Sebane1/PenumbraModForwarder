using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Watchdog.Extensions;
using PenumbraModForwarder.Watchdog.Imports;
using PenumbraModForwarder.Watchdog.Interfaces;

namespace PenumbraModForwarder.Watchdog;

internal class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigurationService _configurationService;
    private readonly IProcessManager _processManager;
    private readonly IConfigurationSetup _configurationSetup;
    private readonly IUpdateService _updateService;

    public Program(
        IConfigurationService configurationService,
        IProcessManager processManager,
        IConfigurationSetup configurationSetup,
        IUpdateService updateService)
    {
        _configurationService = configurationService;
        _processManager = processManager;
        _configurationSetup = configurationSetup;
        _updateService = updateService;
    }

    private static void Main(string[] args)
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
        // Toggle Sentry based on config
        if ((bool)_configurationService.ReturnConfigValue(c => c.Common.EnableSentry))
        {
            DependencyInjection.EnableSentryLogging();
        }
        else
        {
            DependencyInjection.DisableSentryLogging();
        }

        _configurationSetup.CreateFiles();

        // Set initialization flag before starting processes
        ApplicationBootstrapper.SetWatchdogInitialization();

        // Set the environment variable for child processes
        Environment.SetEnvironmentVariable("WATCHDOG_INITIALIZED", "true");

        // Hide the console window on Windows if configured
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HideConsoleWindow();
        }

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var semVersion = version == null
            ? "Local Build"
            : $"{version.Major}.{version.Minor}.{version.Build}";

        // Check for update
        if (_updateService.NeedsUpdateAsync(semVersion).GetAwaiter().GetResult())
        {
            // Run the Updater
            _logger.Info("Update detected, launching updater");
            Environment.Exit(0);
        }

        _processManager.Run();
    }

    private void HideConsoleWindow()
    {
        var showWindow = (bool)_configurationService.ReturnConfigValue(config => config.AdvancedOptions.ShowWatchDogWindow);
        if (showWindow)
        {
            _logger.Info("Showing watchdog window");
            return;
        }

        var handle = DllImports.GetConsoleWindow();
        if (handle == IntPtr.Zero) return;

        _logger.Info("Hiding console window");
        DllImports.ShowWindow(handle, DllImports.SW_HIDE);
        _logger.Info("Console window should now be hidden.");
    }
}