using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reflection;
using PenumbraModForwarder.Common.Attributes;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;
        private ConfigurationModel _configuration;

        // Collections for each category
        public ObservableCollection<SettingGroupViewModel> CommonSettingGroups { get; } = new();
        public ObservableCollection<SettingGroupViewModel> UISettingGroups { get; } = new();
        public ObservableCollection<SettingGroupViewModel> BackgroundWorkerSettingGroups { get; } = new();
        public ObservableCollection<SettingGroupViewModel> AdvancedSettingGroups { get; } = new();

        public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

        // Save Command
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public SettingsViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _configuration = _configurationService.GetConfiguration(); // Receive the shared instance

            // Initialize SaveCommand
            SaveCommand = ReactiveCommand.Create(SaveSettings);

            LoadSettings(_configuration);

            Tabs.Add(new TabItemViewModel { Header = "Common", SettingGroups = CommonSettingGroups });
            Tabs.Add(new TabItemViewModel { Header = "UI", SettingGroups = UISettingGroups });
            Tabs.Add(new TabItemViewModel { Header = "Background Worker", SettingGroups = BackgroundWorkerSettingGroups });
            Tabs.Add(new TabItemViewModel { Header = "Advanced", SettingGroups = AdvancedSettingGroups });
        }

        private void LoadSettings(ConfigurationModel configuration)
        {
            // Load settings for each category
            LoadSettingsFromModel(configuration.Common, CommonSettingGroups);
            LoadSettingsFromModel(configuration.UI, UISettingGroups);
            LoadSettingsFromModel(configuration.BackgroundWorker, BackgroundWorkerSettingGroups);
            LoadSettingsFromModel(configuration.AdvancedOptions, AdvancedSettingGroups);
        }

        private void LoadSettingsFromModel(object model, ObservableCollection<SettingGroupViewModel> targetGroups)
        {
            var modelType = model.GetType();
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !Attribute.IsDefined(prop, typeof(ExcludeFromSettingsUIAttribute)));

            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                string displayName = displayAttr?.Name ?? prop.Name;
                string groupName = displayAttr?.GroupName ?? "General";

                SettingViewModel settingViewModel = CreateSettingViewModel(prop, model, displayName, groupName, modelType);
                if (settingViewModel == null)
                    continue;

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

        private SettingViewModel CreateSettingViewModel(PropertyInfo prop, object model, string displayName, string groupName, Type modelType)
        {
            SettingViewModel settingViewModel = null;

            if (prop.PropertyType == typeof(bool))
            {
                settingViewModel = new BooleanSettingViewModel
                {
                    DisplayName = displayName,
                    GroupName = groupName,
                    TypedValue = prop.GetValue(model) is bool boolValue ? boolValue : false,
                    ModelType = modelType,
                    PropertyInfo = prop,
                };
            }
            else if (prop.PropertyType == typeof(string))
            {
                settingViewModel = new StringSettingViewModel
                {
                    DisplayName = displayName,
                    GroupName = groupName,
                    TypedValue = prop.GetValue(model) as string ?? string.Empty,
                    ModelType = modelType,
                    PropertyInfo = prop,
                };
            }
            else if (prop.PropertyType == typeof(int))
            {
                settingViewModel = new IntSettingViewModel
                {
                    DisplayName = displayName,
                    GroupName = groupName,
                    TypedValue = prop.GetValue(model) is int intValue ? intValue : 0,
                    ModelType = modelType,
                    PropertyInfo = prop,
                };
            }
            // Handle other types similarly...

            else if (prop.PropertyType == typeof(List<string>))
            {
                var list = prop.GetValue(model) as List<string> ?? new List<string>();
                var setting = new ListStringSettingViewModel
                {
                    DisplayName = displayName,
                    GroupName = groupName,
                    ModelType = modelType,
                    PropertyInfo = prop,
                    Items = new ObservableCollection<StringItemViewModel>(
                        list.Select(itemValue => new StringItemViewModel(null) { Value = itemValue })
                    )
                };
                settingViewModel = setting;
            }

            return settingViewModel;
        }

        private object GetTargetModel(ConfigurationModel config, Type modelType)
        {
            return modelType switch
            {
                Type t when t == typeof(CommonConfigurationModel) => config.Common,
                Type t when t == typeof(UIConfigurationModel) => config.UI,
                Type t when t == typeof(BackgroundWorkerConfigurationModel) => config.BackgroundWorker,
                Type t when t == typeof(AdvancedConfigurationModel) => config.AdvancedOptions,
                _ => null,
            };
        }

        private void SaveSettings()
        {
            // Iterate over all settings and update the configuration
            foreach (var tab in Tabs)
            {
                foreach (var group in tab.SettingGroups)
                {
                    foreach (var setting in group.Settings)
                    {
                        UpdateConfigurationSetting(setting);
                    }
                }
            }

            // Pass the updated configuration to the service
            _configurationService.SaveConfiguration(_configuration);
        }

        private void UpdateConfigurationSetting(SettingViewModel setting)
        {
            var targetModel = GetTargetModel(_configuration, setting.ModelType);

            if (targetModel != null && setting.PropertyInfo != null)
            {
                object newValue = null;

                if (setting is BooleanSettingViewModel boolSetting)
                {
                    newValue = boolSetting.TypedValue;
                }
                else if (setting is StringSettingViewModel stringSetting)
                {
                    newValue = stringSetting.TypedValue;
                }
                else if (setting.GetType().IsSubclassOf(typeof(NumberSettingViewModel<>)))
                {
                    newValue = setting.GetType().GetProperty("TypedValue")?.GetValue(setting);
                }
                else if (setting is ListStringSettingViewModel listStringSetting)
                {
                    newValue = listStringSetting.Items.Select(item => item.Value).ToList();
                }

                if (newValue != null)
                {
                    setting.PropertyInfo.SetValue(targetModel, newValue);
                }
            }
        }
    }
}