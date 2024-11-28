using System.Runtime.InteropServices;

namespace PenumbraModForwarder.Watchdog.Imports;

public class DllImports
{
    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [DllImport("kernel32.dll")]
    internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);
    
    internal const int SW_HIDE = 0;
    
    internal delegate bool ConsoleCtrlDelegate(int sig);
}