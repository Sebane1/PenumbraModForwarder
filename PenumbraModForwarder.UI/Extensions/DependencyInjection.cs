using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.UI.ViewModels;

namespace PenumbraModForwarder.UI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add your UI services here
        services.SetupLogging();
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ErrorWindowViewModel>();

        // User Controls
        services.AddTransient<ModsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "UI");
    }
}