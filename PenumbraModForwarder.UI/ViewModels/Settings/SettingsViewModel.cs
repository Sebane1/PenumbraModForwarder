using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using PenumbraModForwarder.Common.Attributes;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class SettingsViewModel : ViewModelBase
{
    
    private readonly IConfigurationService _configurationService;
    
    // Collections for each category
    public ObservableCollection<SettingGroupViewModel> CommonSettingGroups { get; } = new();
    public ObservableCollection<SettingGroupViewModel> UISettingGroups { get; } = new();
    public ObservableCollection<SettingGroupViewModel> BackgroundWorkerSettingGroups { get; } = new();
    public ObservableCollection<SettingGroupViewModel> AdvancedSettingGroups { get; } = new();
    
    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

    public SettingsViewModel(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        var configuration = _configurationService.GetConfiguration();
        LoadSettings(configuration);
        
        Tabs.Add(new TabItemViewModel
        {
            Header = "Common",
            SettingGroups = CommonSettingGroups,
        });
        Tabs.Add(new TabItemViewModel
        {
            Header = "UI",
            SettingGroups = UISettingGroups,
        });
        Tabs.Add(new TabItemViewModel
        {
            Header = "Background Worker",
            SettingGroups = BackgroundWorkerSettingGroups,
        });
        Tabs.Add(new TabItemViewModel
        {
            Header = "Advanced",
            SettingGroups = AdvancedSettingGroups,
        });
    }

    private void LoadSettings(ConfigurationModel configuration)
    {
        // Load settings for each category
        LoadSettingsFromModel(configuration.Common, CommonSettingGroups);
        LoadSettingsFromModel(configuration.UI, UISettingGroups);
        LoadSettingsFromModel(configuration.Watchdog, BackgroundWorkerSettingGroups);
        LoadSettingsFromModel(configuration.AdvancedOptions, AdvancedSettingGroups);
    }

    private void LoadSettingsFromModel(object model, ObservableCollection<SettingGroupViewModel> targetGroups)
    {
        var properties = model.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => !Attribute.IsDefined(prop, typeof(ExcludeFromSettingsUIAttribute)));

        foreach (var prop in properties)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            string displayName = displayAttr?.Name ?? prop.Name;
            string groupName = displayAttr?.GroupName ?? "General";

            SettingViewModel settingViewModel;

            if (prop.PropertyType == typeof(bool))
            {
                settingViewModel = new BooleanSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) is bool boolValue ? boolValue : false,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(string))
            {
                settingViewModel = new StringSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) as string ?? string.Empty,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(int))
            {
                settingViewModel = new IntSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) is int intValue ? intValue : 0,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(double))
            {
                settingViewModel = new DoubleSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) is double doubleValue ? doubleValue : 0.0,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(decimal))
            {
                settingViewModel = new DecimalSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) is decimal decimalValue ? decimalValue : 0m,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(float))
            {
                settingViewModel = new FloatSettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model) is float floatValue ? floatValue : 0f,
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }
            else if (prop.PropertyType == typeof(List<string>))
            {
                var list = prop.GetValue(model) as List<string> ?? new List<string>();
                var observableList = new ObservableCollection<StringItemViewModel>();

                var setting = new ListStringSettingViewModel
                {
                    DisplayName = displayName,
                    Value = observableList,
                    GroupName = groupName,
                    ValueChangedAction = newValue =>
                    {
                        var newList = (newValue as List<string>) ?? new List<string>();
                        prop.SetValue(model, newList);
                    }
                };

                foreach (var itemValue in list)
                {
                    var itemViewModel = new StringItemViewModel(observableList) { Value = itemValue };
                    observableList.Add(itemViewModel);
                }

                settingViewModel = setting;
            }
            else
            {
                // Handle other types if necessary
                settingViewModel = new SettingViewModel
                {
                    DisplayName = displayName,
                    Value = prop.GetValue(model),
                    GroupName = groupName,
                    ValueChangedAction = newValue => prop.SetValue(model, newValue)
                };
            }

            // Find or create the group
            var group = targetGroups.FirstOrDefault(g => g.GroupName == groupName);
            if (group == null)
            {
                group = new SettingGroupViewModel { GroupName = groupName };
                targetGroups.Add(group);
            }

            group.Settings.Add(settingViewModel);
        }
    }
}