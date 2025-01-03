using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;

namespace PenumbraModForwarder.UI.Services;

public class XivLauncherService : IXivLauncherService
{
    private readonly ILogger _logger;
    private const string ConfigFileName = "launcherConfigV3.json";

    public XivLauncherService()
    {
        _logger = Log.ForContext<XivLauncherService>();
    }

    public void EnableAutoStart(bool enable, string appPath, string label)
    {
        _logger.Debug("Entering EnableAutoStart with enable={Enable}, appPath={AppPath}, label={Label}", enable, appPath, label);

        if (enable)
        {
            _logger.Information("Enabling auto-start for {Label}", label);
            AddExternalAppToAddonList(appPath, label);
        }
        else
        {
            _logger.Information("Disabling auto-start for {Label}", label);
            RemoveExternalAppFromAddonList(label);
        }

        _logger.Debug("Exiting EnableAutoStart for label={Label}", label);
    }

    public void EnableAutoStartWatchdog(bool enable)
    {
        _logger.Debug("Entering EnableAutoStartWatchdog with enable={Enable}", enable);

        var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var watchdogPath = Path.Combine(baseDirectory, "PenumbraModForwarder.Watchdog.exe");
            
        const string watchdogLabel = "Penumbra Watchdog";

        EnableAutoStart(enable, watchdogPath, watchdogLabel);

        _logger.Debug("Exiting EnableAutoStartWatchdog");
    }

    private void AddExternalAppToAddonList(string appPath, string label)
    {
        _logger.Debug("Entering AddExternalAppToAddonList with appPath={AppPath}, label={Label}", appPath, label);

        try
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "XIVLauncher"
            );
            var configFilePath = Path.Combine(configDir, ConfigFileName);

            if (!File.Exists(configFilePath))
            {
                _logger.Warning("XIVLauncher config not found; cannot add external app.");
                return;
            }

            var backupPath = configFilePath + ".bak";
            File.Copy(configFilePath, backupPath, true);
            _logger.Debug("Created backup at {BackupPath}", backupPath);

            var jsonContent = File.ReadAllText(configFilePath);
            var root = JObject.Parse(jsonContent);

            var addonToken = root["AddonList"];
            bool storedAsString = false;
            JArray addonArray;

            if (addonToken == null)
            {
                _logger.Debug("No AddonList found, creating new array.");
                addonArray = new JArray();
            }
            else if (addonToken.Type == JTokenType.Array)
            {
                _logger.Debug("AddonList found as array, using existing array.");
                addonArray = (JArray)addonToken;
            }
            else if (addonToken.Type == JTokenType.String)
            {
                _logger.Debug("AddonList found as string, parsing it into an array.");
                storedAsString = true;
                var strValue = addonToken.ToString();
                if (string.IsNullOrWhiteSpace(strValue))
                {
                    addonArray = new JArray();
                }
                else
                {
                    addonArray = JArray.Parse(strValue);
                }
            }
            else
            {
                _logger.Debug("AddonList is in an unexpected format; using empty array instead.");
                addonArray = new JArray();
            }

            var existingObj = FindAddonObjectByLabel(addonArray, label);
            if (existingObj == null)
            {
                _logger.Debug("No existing addon found with label={Label}, creating a new entry.", label);
                var newEntry = new JObject
                {
                    ["IsEnabled"] = true,
                    ["Addon"] = new JObject
                    {
                        ["Path"] = appPath,
                        ["CommandLine"] = "",
                        ["RunAsAdmin"] = false,
                        ["RunOnClose"] = false,
                        ["KillAfterClose"] = false,
                        ["Name"] = label
                    }
                };
                addonArray.Add(newEntry);
            }
            else
            {
                _logger.Debug("Found existing addon with label={Label}, updating path and enabling.", label);
                existingObj["IsEnabled"] = true;
                existingObj["Addon"]["Path"] = appPath;
            }

            if (storedAsString)
            {
                root["AddonList"] = addonArray.ToString(Formatting.None);
            }
            else
            {
                root["AddonList"] = addonArray;
            }

            File.WriteAllText(configFilePath, root.ToString(Formatting.Indented));
            _logger.Debug("Successfully updated XIVLauncher AddonList with external application.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to add external app to XIVLauncher AddonList.");
        }
        finally
        {
            _logger.Debug("Exiting AddExternalAppToAddonList for label={Label}", label);
        }
    }

    private void RemoveExternalAppFromAddonList(string label)
    {
        _logger.Debug("Entering RemoveExternalAppFromAddonList with label={Label}", label);

        try
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "XIVLauncher"
            );
            var configFilePath = Path.Combine(configDir, ConfigFileName);

            if (!File.Exists(configFilePath))
            {
                _logger.Warning("Cannot remove external app. XIVLauncher config not found.");
                return;
            }

            var backupFilePath = configFilePath + ".bak";
            File.Copy(configFilePath, backupFilePath, true);
            _logger.Debug("Created backup at {BackupFilePath}", backupFilePath);

            var jsonContent = File.ReadAllText(configFilePath);
            var root = JObject.Parse(jsonContent);

            var addonToken = root["AddonList"];
            bool storedAsString = false;
            JArray addonArray;

            if (addonToken == null)
            {
                _logger.Debug("No AddonList property found; nothing to remove.");
                return;
            }
            else if (addonToken.Type == JTokenType.Array)
            {
                _logger.Debug("AddonList found as array.");
                addonArray = (JArray)addonToken;
            }
            else if (addonToken.Type == JTokenType.String)
            {
                _logger.Debug("AddonList found as string, parsing it into an array.");
                storedAsString = true;
                var strValue = addonToken.ToString().Trim();
                if (string.IsNullOrEmpty(strValue))
                {
                    addonArray = new JArray();
                }
                else
                {
                    addonArray = JArray.Parse(strValue);
                }
            }
            else
            {
                _logger.Debug("AddonList property is in an unexpected format; cannot remove external app.");
                return;
            }

            var removedCount = 0;
            for (int i = addonArray.Count - 1; i >= 0; i--)
            {
                var entry = addonArray[i];
                var nameToken = entry["Addon"]?["Name"];
                if (nameToken != null && nameToken.ToString() == label)
                {
                    addonArray.RemoveAt(i);
                    removedCount++;
                }
            }

            if (removedCount == 0)
            {
                _logger.Debug("No existing external addons to remove for given label={Label}.", label);
            }
            else
            {
                _logger.Debug("Removed {RemovedCount} external app(s) with label={Label}.", removedCount, label);
            }

            if (storedAsString)
            {
                root["AddonList"] = addonArray.ToString(Formatting.None);
            }
            else
            {
                root["AddonList"] = addonArray;
            }

            File.WriteAllText(configFilePath, root.ToString(Formatting.Indented));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to remove external app from XIVLauncher AddonList.");
        }
        finally
        {
            _logger.Debug("Exiting RemoveExternalAppFromAddonList for label={Label}", label);
        }
    }

    private static JObject? FindAddonObjectByLabel(JArray addonArray, string label)
    {
        foreach (var item in addonArray)
        {
            if (item is JObject obj)
            {
                var addonNode = obj["Addon"];
                if (addonNode?["Name"]?.ToString() == label)
                {
                    return obj;
                }
            }
        }
        return null;
    }
}