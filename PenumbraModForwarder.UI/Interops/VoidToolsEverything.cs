using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.UI.Interops
{
    public class VoidToolsEverything : IVoidToolsEverything
    {
        private readonly ILogger<VoidToolsEverything> _logger;
        private readonly string _dllPath;

        public VoidToolsEverything(ILogger<VoidToolsEverything> logger)
        {
            _logger = logger;
            
            _dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", Environment.Is64BitProcess ? "Everything64.dll" : "Everything32.dll");

            _logger.LogDebug($"Attempting to load DLL from: {_dllPath}");
            var handle = LoadLibrary(_dllPath);
            
            if (handle != IntPtr.Zero) return;
            
            var errorCode = Marshal.GetLastWin32Error();
            
            _logger.LogError($"Failed to load DLL. Error code: {errorCode}");
            throw new Exception($"Could not load the DLL. Error code: {errorCode}");

        }
        
        public void SetSearch(string searchString)
        {
            Everything_SetSearchW(searchString);
        }

        public bool Query(bool wait)
        {
            return Everything_QueryW(wait);
        }

        public int GetNumResults()
        {
            return Everything_GetNumResults();
        }

        public string GetResultFullPathName(int index)
        {
            var resultPath = new StringBuilder(260); // MAX_PATH
            Everything_GetResultFullPathName(index, resultPath, resultPath.Capacity);
            return resultPath.ToString();
        }

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        private static extern int Everything_SetSearchW(string lpSearchString);

        [DllImport("Everything64.dll")]
        private static extern bool Everything_QueryW(bool bWait);

        [DllImport("Everything64.dll")]
        private static extern int Everything_GetNumResults();

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr Everything_GetResultFullPathName(int nIndex, StringBuilder lpString, int nMaxCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);
    }
}
