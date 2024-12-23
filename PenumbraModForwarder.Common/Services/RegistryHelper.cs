using Microsoft.Win32;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Common.Services
{
    public class RegistryHelper : IRegistryHelper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Indicates whether the registry is supported on the current platform.
        /// </summary>
        public bool IsRegistrySupported => OperatingSystem.IsWindows();

        public RegistryHelper()
        {
            _logger = Log.ForContext<RegistryHelper>();
        }

        /// <summary>
        /// Retrieves the TexTools installation location from the Windows Registry.
        /// </summary>
        /// <returns>The installation path of TexTools, or null if not found.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when called on non-Windows platforms.</exception>
        public string GetTexToolRegistryValue()
        {
            if (!IsRegistrySupported)
            {
                throw new PlatformNotSupportedException("Registry access is only supported on Windows.");
            }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryConsts.RegistryPath);
                var value = key?.GetValue("InstallLocation")?.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    _logger.Warning("Registry value not found at {Path}", RegistryConsts.RegistryPath);
                    return null;
                }
                return value;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get registry value for {Path}", RegistryConsts.RegistryPath);
                throw new Exception($"Failed to get registry value for {RegistryConsts.RegistryPath}", e);
            }
        }
    }
}