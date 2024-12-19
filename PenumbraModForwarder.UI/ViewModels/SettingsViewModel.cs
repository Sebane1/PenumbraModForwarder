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

        // Get all configuration model properties
        var configModelProperties = configurationModel.GetType().GetProperties()
            .Where(p => p.PropertyType.Namespace == "PenumbraModForwarder.Common.Models");

        foreach (var prop in configModelProperties)
        {
            var modelInstance = prop.GetValue(configurationModel);
            LoadPropertiesFromModel(modelInstance);
        }
    }

    private void LoadPropertiesFromModel(object model)
    {
        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            // Skip properties marked with [ExcludeFromSettingsUI]
            if (prop.GetCustomAttribute<ExcludeFromSettingsUIAttribute>() != null)
                continue;

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            string displayName = displayAttr?.Name ?? prop.Name;
            string groupName = displayAttr?.GroupName ?? "General";

            var descriptor = new ConfigurationPropertyDescriptor
            {
                DisplayName = displayName,
                PropertyInfo = prop,
                ModelInstance = model,
                GroupName = groupName
            };

            // Set initial value
            descriptor.Value = prop.GetValue(model);

            // Handle commands if necessary
            if (prop.PropertyType == typeof(string) && displayName.Contains("Path"))
            {
                descriptor.BrowseCommand = ReactiveCommand.CreateFromTask(() => ExecuteBrowseCommand(descriptor));
            }

            if (prop.PropertyType == typeof(List<string>))
            {
                descriptor.BrowseCommand = ReactiveCommand.CreateFromTask(() => ExecuteBrowseCommand(descriptor));
                // No need to assign RemovePathCommand here; it's handled in PathItemViewModel
            }

            var group = Groups.FirstOrDefault(g => g.GroupName == groupName);
            if (group == null)
            {
                group = new ConfigurationGroup(groupName);
                Groups.Add(group);
            }

            group.Properties.Add(descriptor);

            // Subscribe to changes and save configuration
            descriptor.WhenAnyValue(d => d.Value)
                .Skip(1) // Skip the initial value
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => SaveSettings());
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

    private void SaveSettings()
    {
        try
        {
            _configurationService.UpdateConfigValue(_ => { /* No action needed since model instances are updated */ });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving settings");
        }
    }
}