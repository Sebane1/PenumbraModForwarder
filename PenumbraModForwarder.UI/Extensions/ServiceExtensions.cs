using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Services;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.UI.Extensions;

public static class ServiceExtensions
{
    public static IServiceProvider Configuration()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ConfigureViews(services);
        ConfigureLogging(services);
        ConfigureAutoMapper(services);

        return services.BuildServiceProvider();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Registering services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IFileWatcher, FileWatcher>();
        services.AddSingleton<IArchiveHelperService, ArchiveHelperService>();
        services.AddSingleton<IFileHandlerService, FileHandlerService>();
        services.AddSingleton<IFileSelector, FileSelector>();
        services.AddTransient<IPenumbraApi, PenumbraApi>();
        services.AddSingleton<IRegistryHelper, RegistryHelper>();
        services.AddSingleton<IPenumbraInstallerService, PenumbraInstallerService>();
        services.AddTransient<IUpdateService, UpdateService>();
        services.AddSingleton<IErrorWindowService, ErrorWindowService>();
        services.AddSingleton<IProcessHelperService, ProcessHelperService>();
        services.AddSingleton<IArkService, ArkService>();
        services.AddSingleton<ISystemTrayManager, SystemTrayManager>();
        services.AddSingleton<IAssociateFileTypeService, AssociateFileTypesService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IShortcutService, ShortcutService>();
        services.AddSingleton<IResourceManager, ResourceManager>();
        services.AddSingleton<ITexToolsHelper, TexToolsHelper>();
        services.AddSingleton<IProgressWindowService, ProgressWindowService>();
    }
    
    private static void ConfigureViews(IServiceCollection services)
    {
        // Registering ViewModels and Views as transient
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<FileSelectViewModel>();
        services.AddTransient<FileSelect>();
        services.AddTransient<ErrorWindowViewModel>();
        services.AddTransient<ErrorWindow>();
        services.AddTransient<ProgressWindowViewModel>();
        services.AddTransient<ProgressWindow>();
    }
    
    private static void ConfigureLogging(IServiceCollection services)
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PenumbraModForwarder", "logs");
        Directory.CreateDirectory(appDataPath);

#if DEBUG
        var minimumLevel = LogEventLevel.Debug;
#else
    var minimumLevel = LogEventLevel.Information;
#endif
    
        var logFilePath = Path.Combine(appDataPath, "log.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.Console()
            .WriteTo.File(logFilePath,
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,  
                rollOnFileSizeLimit: true 
            )
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }


    
    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        // AutoMapper configuration
        services.AddAutoMapper(typeof(MappingProfile)); 
    }
}