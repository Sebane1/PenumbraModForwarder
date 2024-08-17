using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var serviceProvider = Extensions.ServiceExtensions.Configuration();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(serviceProvider.GetRequiredService<MainWindow>());
    }
}