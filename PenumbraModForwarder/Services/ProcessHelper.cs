using System.Diagnostics;

namespace PenumbraModForwarder.Services;

public static class ProcessHelper
{
    public static void OpenWebsite(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}