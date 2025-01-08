using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NLog;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.Services
{
    public class RegistryHelper : IRegistryHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // Base path in HKEY_CURRENT_USER for file associations
        private const string HkcuClassesPath = @"Software\Classes";

        // Classes subkey for our custom file type: "PenumbraModpackFile"
        private const string PenumbraFileKey = "PenumbraModpackFile";

        // Relative paths under "PenumbraModpackFile"
        private const string DefaultIconPath = @"DefaultIcon";
        private const string ShellOpenCommandPath = @"shell\open\command";

        // Startup registry key
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        /// <summary>
        /// Creates or updates a file association in HKCU for the given extensions.
        /// This method sets up:
        /// - an extension mapping (e.g. ".pmp" => "PenumbraModpackFile")
        /// - a DefaultIcon
        /// - the shell\open\command
        /// so that double-clicking on these file extensions invokes the specified application.
        /// </summary>
        /// <param name="extensions">File extensions to associate (".pmp", ".ttmp2" etc.).</param>
        /// <param name="applicationPath">Path to the executable that should handle these files.</param>
        public void CreateFileAssociation(IEnumerable<string> extensions, string applicationPath)
        {
            _logger.Debug("Entering CreateFileAssociation with {ApplicationPath}", applicationPath);

            if (string.IsNullOrWhiteSpace(applicationPath) || !File.Exists(applicationPath))
            {
                _logger.Error("Invalid application path for file association: {Path}", applicationPath);
                return;
            }

            try
            {
                foreach (var extension in extensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        _logger.Debug("Skipping an empty or null extension.");
                        continue;
                    }

                    var extensionKeyPath = Path.Join(HkcuClassesPath, extension.TrimStart('.'));
                    _logger.Debug("Creating subkey for extension: {ExtensionKeyPath}", extensionKeyPath);

                    using var extKey = Registry.CurrentUser.CreateSubKey(extensionKeyPath);
                    extKey?.SetValue("", PenumbraFileKey);
                }

                // Provide a default icon for these files (optional)
                var defaultIconSubkeyPath = Path.Join(HkcuClassesPath, PenumbraFileKey, DefaultIconPath);
                _logger.Debug("Creating icon subkey at {IconSubkeyPath}", defaultIconSubkeyPath);

                using (var iconKey = Registry.CurrentUser.CreateSubKey(defaultIconSubkeyPath))
                {
                    iconKey?.SetValue("", $"{applicationPath},0");
                }

                // Provide the open command
                var openCmdSubkeyPath = Path.Join(HkcuClassesPath, PenumbraFileKey, ShellOpenCommandPath);
                _logger.Debug("Creating shell open command subkey at {OpenCmdSubkeyPath}", openCmdSubkeyPath);

                using (var commandKey = Registry.CurrentUser.CreateSubKey(openCmdSubkeyPath))
                {
                    commandKey?.SetValue("", $"\"{applicationPath}\" \"%1\"");
                }

                _logger.Info("Successfully created file associations for {Application}", applicationPath);

                // Notify Windows of the change in file associations
                _logger.Debug("Sending SHChangeNotify about file association changes.");
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error creating file associations in the registry.");
            }
            finally
            {
                _logger.Debug("Exiting CreateFileAssociation.");
            }
        }

        /// <summary>
        /// Removes file associations for the provided extensions and
        /// cleans up the "PenumbraModpackFile" subkey under HKCU\Software\Classes.
        /// </summary>
        /// <param name="extensions">File extensions to remove (".pmp", ".ttmp2" etc.).</param>
        public void RemoveFileAssociation(IEnumerable<string> extensions)
        {
            _logger.Debug("Entering RemoveFileAssociation.");

            try
            {
                foreach (var extension in extensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        _logger.Debug("Skipping an empty or null extension in removal.");
                        continue;
                    }

                    var extensionKeyPath = Path.Join(HkcuClassesPath, extension.TrimStart('.'));
                    _logger.Debug("Removing subkey: {ExtensionKeyPath}", extensionKeyPath);

                    Registry.CurrentUser.DeleteSubKeyTree(extensionKeyPath, throwOnMissingSubKey: false);
                }

                var mainFileKeyPath = Path.Join(HkcuClassesPath, PenumbraFileKey);
                _logger.Debug("Removing main file key at {MainFileKeyPath}", mainFileKeyPath);
                Registry.CurrentUser.DeleteSubKeyTree(mainFileKeyPath, false);

                _logger.Info("File associations removed successfully.");

                _logger.Debug("Sending SHChangeNotify about file association removal.");
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error removing file associations from the registry.");
            }
            finally
            {
                _logger.Debug("Exiting RemoveFileAssociation.");
            }
        }

        /// <summary>
        /// Places or updates a registry key under "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
        /// which tells Windows to launch the specified application at logon.
        /// </summary>
        /// <param name="appName">Display name for the registry entry (e.g. "PenumbraForwarder").</param>
        /// <param name="appPath">Full path to the executable.</param>
        public void AddApplicationToStartup(string appName, string appPath)
        {
            _logger.Debug("Entering AddApplicationToStartup with AppName={AppName}, AppPath={AppPath}",
                appName, appPath);

            if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(appPath))
            {
                _logger.Error("Invalid arguments for adding startup registry key. AppName: {AppName}, AppPath: {AppPath}",
                    appName, appPath);
                return;
            }

            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

                _logger.Debug("Writing startup key for {AppName}", appName);
                registryKey?.SetValue(appName, $"\"{appPath}\"");

                _logger.Info("Application {AppName} added to startup at {AppPath}", appName, appPath);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error adding application {AppName} to startup", appName);
            }
            finally
            {
                _logger.Debug("Exiting AddApplicationToStartup.");
            }
        }

        /// <summary>
        /// Removes the specified entry from "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
        /// stopping Windows from launching it at logon.
        /// </summary>
        /// <param name="appName">The registry entry name to remove.</param>
        public void RemoveApplicationFromStartup(string appName)
        {
            _logger.Debug("Entering RemoveApplicationFromStartup with AppName={AppName}", appName);

            if (string.IsNullOrWhiteSpace(appName))
            {
                _logger.Error("Invalid argument for removing application from startup: AppName is null or empty.");
                return;
            }

            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

                _logger.Debug("Removing startup key for {AppName}", appName);
                registryKey?.DeleteValue(appName, throwOnMissingValue: false);

                _logger.Info("Application {AppName} removed from startup.", appName);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error removing application {AppName} from startup", appName);
            }
            finally
            {
                _logger.Debug("Exiting RemoveApplicationFromStartup.");
            }
        }
    }
}