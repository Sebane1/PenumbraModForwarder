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
        if (enable)
        {
            AddExternalAppToAddonList(appPath, label);
        }
        else
        {
            RemoveExternalAppFromAddonList(label);
        }
    }

    public void EnableAutoStartWatchdog(bool enable)
    {
        var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var watchdogPath = Path.Combine(baseDirectory, "PenumbraModForwarder.Watchdog.exe");
        
        const string watchdogLabel = "Penumbra Watchdog";

        EnableAutoStart(enable, watchdogPath, watchdogLabel);
    }

    private void AddExternalAppToAddonList(string appPath, string label)
    {
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

            // Backup first
            var backupPath = configFilePath + ".bak";
            File.Copy(configFilePath, backupPath, true);

            var jsonContent = File.ReadAllText(configFilePath);
            var root = JObject.Parse(jsonContent);

            // This property might be an array or a string containing serialized JSON
            var addonToken = root["AddonList"];
            bool storedAsString = false;
            JArray addonArray;

            if (addonToken == null)
            {
                addonArray = new JArray();
            }
            else if (addonToken.Type == JTokenType.Array)
            {
                addonArray = (JArray)addonToken;
            }
            else if (addonToken.Type == JTokenType.String)
            {
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
                _logger.Debug("AddonList is in an unexpected format; re-initializing it as an empty array.");
                addonArray = new JArray();
            }

            var existingObj = FindAddonObjectByLabel(addonArray, label);
            if (existingObj == null)
            {
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
    }

    private void RemoveExternalAppFromAddonList(string label)
    {
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

            // Backup
            var backupFilePath = configFilePath + ".bak";
            File.Copy(configFilePath, backupFilePath, true);

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
                addonArray = (JArray)addonToken;
            }
            else if (addonToken.Type == JTokenType.String)
            {
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
                _logger.Debug("No existing external addons to remove for given label.");
            }
            else
            {
                _logger.Debug($"Removed {removedCount} external app(s) with label `{label}` from XIVLauncher AddonList.");
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