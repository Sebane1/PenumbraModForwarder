using System.Runtime.InteropServices;

namespace PenumbraModForwarder.Common.Interop;

public class Imports
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void SHChangeNotify(long wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
}