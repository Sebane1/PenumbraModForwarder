using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PenumbraModForwarder.Common.Attributes;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Helpers;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigurationService _configurationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IWebSocketClient _webSocketClient;

    public ObservableCollection<ConfigurationGroup> Groups { get; } = new();

    public SettingsViewModel(
        IConfigurationService configurationService,
        IFileDialogService fileDialogService,
        IWebSocketClient webSocketClient)
    {
        _configurationService = configurationService;
        _fileDialogService = fileDialogService;
        _webSocketClient = webSocketClient;

        LoadConfigurationSettings();
    }

    private void LoadConfigurationSettings()
    {
        var configurationModel = _configurationService.GetConfiguration();
        LoadPropertiesFromModel(configurationModel);
    }

    private void LoadPropertiesFromModel(
        object model,
        ConfigurationPropertyDescriptor parentDescriptor = null,
        string parentGroupName = null)
    {
        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            // Skip properties marked with [ExcludeFromSettingsUI]
            if (prop.GetCustomAttribute<ExcludeFromSettingsUIAttribute>() != null)
                continue;

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttr?.Name ?? prop.Name;
            var description = displayAttr?.Description;
            var groupName = displayAttr?.GroupName ?? parentGroupName ?? "General";

            _logger.Debug(
                "Processing property '{PropertyName}' of type '{PropertyType}' in group '{GroupName}'",
                prop.Name,
                prop.PropertyType.Name,
                groupName);

            if (IsNestedModel(prop.PropertyType))
            {
                var nestedModelInstance = prop.GetValue(model);
                var nestedDescriptor = new ConfigurationPropertyDescriptor
                {
                    DisplayName = displayName,
                    Description = description,
                    PropertyInfo = prop,
                    ModelInstance = model,
                    ParentDescriptor = parentDescriptor,
                    GroupName = groupName
                };

                LoadPropertiesFromModel(nestedModelInstance, nestedDescriptor, groupName);
            }
            else
            {
                var descriptor = new ConfigurationPropertyDescriptor
                {
                    DisplayName = displayName,
                    Description = description,
                    PropertyInfo = prop,
                    ModelInstance = model,
                    ParentDescriptor = parentDescriptor,
                    GroupName = groupName
                };

                descriptor.Value = prop.GetValue(model);

                // If the property is a path, attach a "BrowseCommand"
                if ((prop.PropertyType == typeof(string) || prop.PropertyType == typeof(List<string>))
                    && displayName.Contains("Path", StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.BrowseCommand = ReactiveCommand.CreateFromTask(
                        () => ExecuteBrowseCommand(descriptor)
                    );
                }

                var group = Groups.FirstOrDefault(g => g.GroupName == groupName);
                if (group == null)
                {
                    group = new ConfigurationGroup(groupName);
                    Groups.Add(group);
                }

                // Check for duplicates
                var existingDescriptor = group.Properties
                    .FirstOrDefault(d => d.PropertyInfo.Name == descriptor.PropertyInfo.Name);

                if (existingDescriptor == null)
                {
                    group.Properties.Add(descriptor);
                }
                else
                {
                    _logger.Warn(
                        "Property '{PropertyName}' is already added to group '{GroupName}'. Skipping duplicate.",
                        descriptor.PropertyInfo.Name,
                        groupName
                    );
                }

                // Subscribe to changes
                descriptor.WhenAnyValue(d => d.Value)
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => SaveSettings(descriptor));
            }
        }
    }

    private bool IsNestedModel(Type type)
    {
        return type.Namespace == "PenumbraModForwarder.Common.Models"
               && type.IsClass
               && !type.IsPrimitive
               && !type.IsEnum
               && type != typeof(string)
               && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private async Task ExecuteBrowseCommand(ConfigurationPropertyDescriptor descriptor)
    {
        try
        {
            string initialDirectory = null;

            if (descriptor.Value is string path && !string.IsNullOrEmpty(path))
            {
                initialDirectory = System.IO.Path.GetDirectoryName(path);
            }
            else if (descriptor.Value is List<string> paths && paths.Any())
            {
                initialDirectory = System.IO.Path.GetDirectoryName(paths.Last());
            }

            if (descriptor.PropertyInfo.PropertyType == typeof(string))
            {
                var selectedPath = await _fileDialogService.OpenFolderAsync(
                    initialDirectory,
                    $"Select {descriptor.DisplayName}"
                );

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    descriptor.Value = selectedPath;
                }
            }
            else if (descriptor.PropertyInfo.PropertyType == typeof(List<string>))
            {
                var selectedPaths = await _fileDialogService.OpenFoldersAsync(
                    initialDirectory,
                    $"Select {descriptor.DisplayName}"
                );

                if (selectedPaths != null && selectedPaths.Any())
                {
                    var existingPaths = descriptor.Value as List<string> ?? new List<string>();
                    var newPathsList = existingPaths.Union(selectedPaths).ToList();
                    descriptor.Value = newPathsList;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing browse command for {DisplayName}", descriptor.DisplayName);
        }
    }

    private void SaveSettings(ConfigurationPropertyDescriptor descriptor)
    {
        try
        {
            var propertyPath = GetPropertyPath(descriptor);

            _configurationService.UpdateConfigValue(
                config => SetNestedPropertyValue(config, descriptor),
                propertyPath,
                descriptor.Value
            );

            var taskId = Guid.NewGuid().ToString();
            var configurationChange = new
            {
                PropertyPath = propertyPath,
                NewValue = descriptor.Value
            };

            var message = WebSocketMessage.CreateStatus(
                taskId,
                WebSocketMessageStatus.InProgress,
                $"Configuration changed: {propertyPath}"
            );

            message.Type = WebSocketMessageType.ConfigurationChange;
            message.Message = JsonConvert.SerializeObject(configurationChange);

            _ = _webSocketClient.SendMessageAsync(message, "/config").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving settings");
        }
    }

    private void SetNestedPropertyValue(ConfigurationModel config, ConfigurationPropertyDescriptor descriptor)
    {
        var propertyPath = GetPropertyPath(descriptor);
        var properties = propertyPath.Split('.');

        object currentObject = config;
        for (int i = 0; i < properties.Length; i++)
        {
            var propertyName = properties[i];
            var propertyInfo = currentObject.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new Exception(
                    $"Property '{propertyName}' not found on object of type '{currentObject.GetType().Name}'"
                );
            }

            if (i == properties.Length - 1)
            {
                object finalValue;

                if (propertyInfo.PropertyType == typeof(int) && descriptor.Value is decimal decimalVal)
                {
                    finalValue = Convert.ToInt32(decimalVal);
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    finalValue = descriptor.Value?.ToString();
                }
                else if (propertyInfo.PropertyType == typeof(List<string>)
                         && descriptor.Value is IEnumerable<string> stringEnum)
                {
                    finalValue = new List<string>(stringEnum);
                }
                else
                {
                    finalValue = Convert.ChangeType(descriptor.Value, propertyInfo.PropertyType);
                }

                propertyInfo.SetValue(currentObject, finalValue);

                _logger.Debug(
                    "Set value of property '{PropertyPath}' to '{Value}'",
                    propertyPath,
                    finalValue
                );
            }
            else
            {
                currentObject = propertyInfo.GetValue(currentObject);
            }
        }
    }

    private string GetPropertyPath(ConfigurationPropertyDescriptor descriptor)
    {
        var pathSegments = new List<string>();
        var currentDescriptor = descriptor;

        while (currentDescriptor != null)
        {
            pathSegments.Insert(0, currentDescriptor.PropertyInfo.Name);
            currentDescriptor = currentDescriptor.ParentDescriptor;
        }

        return string.Join(".", pathSegments);
    }
}