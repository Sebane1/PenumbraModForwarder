using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Attributes;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Helpers;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;
using Serilog;

namespace PenumbraModForwarder.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileDialogService _fileDialogService;

    public ObservableCollection<ConfigurationGroup> Groups { get; } = new();

    public SettingsViewModel(IConfigurationService configurationService, IFileDialogService fileDialogService)
    {
        _configurationService = configurationService;
        _fileDialogService = fileDialogService;

        LoadConfigurationSettings();
    }

    private void LoadConfigurationSettings()
    {
        var configurationModel = _configurationService.GetConfiguration();

        // Load all properties recursively
        LoadPropertiesFromModel(configurationModel);
    }

    private void LoadPropertiesFromModel(object model, ConfigurationPropertyDescriptor parentDescriptor = null, string parentGroupName = null)
    {
        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            // Skip properties marked with [ExcludeFromSettingsUI]
            if (prop.GetCustomAttribute<ExcludeFromSettingsUIAttribute>() != null)
                continue;

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            string displayName = displayAttr?.Name ?? prop.Name;
            string groupName = displayAttr?.GroupName ?? parentGroupName ?? "General";

            var propertyType = prop.PropertyType;

            if (propertyType.Namespace == "PenumbraModForwarder.Common.Models")
            {
                // Nested model, create a descriptor for it
                var nestedModelInstance = prop.GetValue(model);

                var nestedDescriptor = new ConfigurationPropertyDescriptor
                {
                    DisplayName = displayName,
                    PropertyInfo = prop,
                    ModelInstance = model,
                    ParentDescriptor = parentDescriptor,
                    GroupName = groupName
                };

                // Recurse into the nested model
                LoadPropertiesFromModel(nestedModelInstance, nestedDescriptor, groupName);
            }
            else
            {
                var descriptor = new ConfigurationPropertyDescriptor
                {
                    DisplayName = displayName,
                    PropertyInfo = prop,
                    ModelInstance = model,
                    ParentDescriptor = parentDescriptor,
                    GroupName = groupName
                };

                // Set initial value
                descriptor.Value = prop.GetValue(model);

                // Handle commands if necessary
                if (prop.PropertyType == typeof(string) && displayName.Contains("Path", StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.BrowseCommand = ReactiveCommand.CreateFromTask(() => ExecuteBrowseCommand(descriptor));
                }

                if (prop.PropertyType == typeof(List<string>))
                {
                    descriptor.BrowseCommand = ReactiveCommand.CreateFromTask(() => ExecuteBrowseCommand(descriptor));
                }

                var group = Groups.FirstOrDefault(g => g.GroupName == groupName);
                if (group == null)
                {
                    group = new ConfigurationGroup(groupName);
                    Groups.Add(group);
                }

                group.Properties.Add(descriptor);

                // Subscribe to changes and pass descriptor to SaveSettings
                descriptor.WhenAnyValue(d => d.Value)
                    .Skip(1) // Skip the initial value
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => SaveSettings(descriptor));
            }
        }
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

            if (descriptor.PropertyInfo.PropertyType == typeof(string))
            {
                var selectedPath = await _fileDialogService.OpenFolderAsync(initialDirectory, $"Select {descriptor.DisplayName}");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    descriptor.Value = selectedPath;
                }
            }
            else if (descriptor.PropertyInfo.PropertyType == typeof(List<string>))
            {
                var selectedPaths = await _fileDialogService.OpenFoldersAsync(initialDirectory, $"Select {descriptor.DisplayName}");
                if (selectedPaths != null && selectedPaths.Any())
                {
                    var pathsList = descriptor.Value as List<string> ?? new List<string>();
                    pathsList.AddRange(selectedPaths);
                    descriptor.Value = pathsList;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing browse command for {DisplayName}", descriptor.DisplayName);
        }
    }

    private void SaveSettings(ConfigurationPropertyDescriptor descriptor)
    {
        try
        {
            _configurationService.UpdateConfigValue(config =>
            {
                SetNestedPropertyValue(config, descriptor);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving settings");
        }
    }

    private void SetNestedPropertyValue(ConfigurationModel config, ConfigurationPropertyDescriptor descriptor)
    {
        var propertyPath = GetPropertyPath(descriptor);
        var properties = propertyPath.Split('.');

        object currentObject = config;
        PropertyInfo propertyInfo = null;

        for (int i = 0; i < properties.Length; i++)
        {
            var propertyName = properties[i];
            propertyInfo = currentObject.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new Exception($"Property '{propertyName}' not found on object of type '{currentObject.GetType().Name}'");
            }

            if (i == properties.Length - 1)
            {
                // Last property - set the value
                propertyInfo.SetValue(currentObject, descriptor.Value);
            }
            else
            {
                // Navigate to the next level
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