using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
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

        public Program()
        {
            _logger = Log.ForContext<Program>();
        }

        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        public void Run(string[] args)
        {
            var services = new ServiceCollection();
            services.AddApplicationServices();
            var serviceProvider = services.BuildServiceProvider();

            var configService = serviceProvider.GetService<IConfigurationSetup>();
            configService.CreateFiles();

            // Set initialization flag before starting processes
            ApplicationBootstrapper.SetWatchdogInitialization();

            // Set the environment variable for child processes
            Environment.SetEnvironmentVariable("WATCHDOG_INITIALIZED", "true");

            // Hide the console window on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                HideConsoleWindow();
            }

            var processManager = serviceProvider.GetService<IProcessManager>();
            processManager.Run();
        }

        void HideConsoleWindow()
        {
            var handle = DllImports.GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                _logger.Information("Hiding console window");
                DllImports.ShowWindow(handle, DllImports.SW_HIDE);
                _logger.Information("Console window should now be hidden.");
            }
        }
    }
}