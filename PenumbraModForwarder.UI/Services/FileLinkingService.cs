using System;
using System.IO;
using System.Reflection;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;

namespace PenumbraModForwarder.UI.Services
{
    public class FileLinkingService : IFileLinkingService
    {
        private readonly IRegistryHelper _registryHelper;
        private readonly ILogger _logger;

        private const string ConsoleToolingExe = "PenumbraModForwarder.ConsoleTooling.exe";
        private const string LauncherExe       = "PenumbraModForwarder.Launcher.exe";
        private const string StartupAppName    = "PenumbraForwarderLauncher";

        private readonly string _consoleToolingPath;
        private readonly string _launcherPath;

        public FileLinkingService(IRegistryHelper registryHelper)
        {
            _logger = Log.Logger.ForContext<FileLinkingService>();
            _registryHelper = registryHelper;

            // Determine the current directory (where this .dll or .exe is running from).
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                             ?? AppContext.BaseDirectory;

            // Build absolute paths to both exes.
            _consoleToolingPath = Path.Combine(currentDir, ConsoleToolingExe);
            _launcherPath       = Path.Combine(currentDir, LauncherExe);
        }

        /// <summary>
        /// Enables file linking for the known allowed extensions, using ConsoleTooling to open them.
        /// </summary>
        public void EnableFileLinking()
        {
            if (!File.Exists(_consoleToolingPath))
            {
                _logger.Error($"Console Tooling not found: {_consoleToolingPath}");
                return;
            }

            // Register the .pmp, .ttmp, .ttmp2, .zip, .rar, .7z associations.
            _registryHelper.CreateFileAssociation(FileExtensionsConsts.AllowedExtensions, _consoleToolingPath);
        }

        /// <summary>
        /// Disables file linking for the known allowed extensions.
        /// </summary>
        public void DisableFileLinking()
        {
            _registryHelper.RemoveFileAssociation(FileExtensionsConsts.AllowedExtensions);
        }

        /// <summary>
        /// Adds the launcher to Windows startup (HKCU\Software\Microsoft\Windows\CurrentVersion\Run).
        /// </summary>
        public void EnableStartup()
        {
            if (!File.Exists(_launcherPath))
            {
                _logger.Error($"Launcher not found: {_launcherPath}");
                return;
            }

            _registryHelper.AddApplicationToStartup(StartupAppName, _launcherPath);
        }

        /// <summary>
        /// Removes the launcher from Windows startup.
        /// </summary>
        public void DisableStartup()
        {
            _registryHelper.RemoveApplicationFromStartup(StartupAppName);
        }
    }
}