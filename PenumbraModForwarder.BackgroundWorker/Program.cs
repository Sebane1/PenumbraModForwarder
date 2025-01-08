using NLog;
using PenumbraModForwarder.BackgroundWorker.Extensions;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static void Main(string[] args)
    {
        bool isNewInstance;
        using (new Mutex(true, "PenumbraModForwarder.BackgroundWorker", out isNewInstance))
        {
            if (!isNewInstance)
            {
                _logger.Info("Another instance of PenumbraModForwarder.BackgroundWorker is already running. Exiting...");
                return;
            }

            var builder = Host.CreateApplicationBuilder(args);

            bool isInitializedByWatchdog = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";

#if DEBUG
            // In debug mode, mark as initialized and assign a default port if none is provided
            isInitializedByWatchdog = true;
            if (args.Length == 0)
            {
                args = new string[] { "12345" }; // Fixed port for debugging
            }
#endif

            _logger.Info("Application initialized by watchdog: {IsInitialized}", isInitializedByWatchdog);

            if (!isInitializedByWatchdog)
            {
                _logger.Warn("Application must be started through the main executable.");
                return;
            }

            if (args.Length == 0)
            {
                _logger.Fatal("No port specified for the BackgroundWorker.");
                return;
            }

            if (!int.TryParse(args[0], out var port))
            {
                _logger.Fatal("Invalid port specified: {PortArg}", args[0]);
                return;
            }

            _logger.Info("Starting BackgroundWorker on port {Port}", port);

            // Register your application services (including logging, etc.)
            builder.Services.AddApplicationServices(port);

            var host = builder.Build();
            host.Run();
        }
    }
}