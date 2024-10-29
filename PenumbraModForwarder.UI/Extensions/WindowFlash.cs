using System.Runtime.InteropServices;

namespace PenumbraModForwarder.UI.Extensions;

public static class WindowFlash
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    // Flash both window and taskbar
    private const uint FLASHW_ALL = 3;
    // Flash the window until focus
    private const uint FLASHW_TIMERNOFG = 12;

    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }
    
    public static bool FlashNotification(this Form form)
    {
        IntPtr hWnd = form.Handle;
        FLASHWINFO fInfo = new FLASHWINFO();

        fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
        fInfo.hwnd = hWnd;
        fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
        fInfo.uCount = uint.MaxValue;
        fInfo.dwTimeout = 0;

        return FlashWindowEx(ref fInfo);
    }
}