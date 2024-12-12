using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public SettingsViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _configuration = _configurationService.GetConfiguration();
            LoadSettings(_configuration);

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

        private object GetTargetModel(ConfigurationModel config, object currentModel)
        {
            if (currentModel == _configuration.Common) return config.Common;
            if (currentModel == _configuration.UI) return config.UI;
            if (currentModel == _configuration.Watchdog) return config.Watchdog;
            if (currentModel == _configuration.AdvancedOptions) return config.AdvancedOptions;

            return null;
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
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) is bool boolValue ? boolValue : false,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, Convert.ToBoolean(newValue));
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(string))
                {
                    settingViewModel = new StringSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) as string ?? string.Empty,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, newValue);
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(int))
                {
                    settingViewModel = new IntSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) is int intValue ? intValue : 0,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, Convert.ToInt32(newValue));
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(double))
                {
                    settingViewModel = new DoubleSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) is double doubleValue ? doubleValue : 0.0,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, Convert.ToDouble(newValue));
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(decimal))
                {
                    settingViewModel = new DecimalSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) is decimal decimalValue ? decimalValue : 0m,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, Convert.ToDecimal(newValue));
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(float))
                {
                    settingViewModel = new FloatSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        TypedValue = prop.GetValue(model) is float floatValue ? floatValue : 0f,
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, Convert.ToSingle(newValue));
                            });
                        }
                    };
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    var list = prop.GetValue(model) as List<string> ?? new List<string>();

                    var setting = new ListStringSettingViewModel
                    {
                        DisplayName = displayName,
                        GroupName = groupName,
                        Items = new ObservableCollection<StringItemViewModel>(),
                        ValueChangedAction = newValue =>
                        {
                            _configurationService.UpdateConfigValue(config =>
                            {
                                var targetModel = GetTargetModel(config, model);
                                prop.SetValue(targetModel, newValue);
                            });
                        }
                    };

                    // Subscribe to item value changes
                    setting.Items.CollectionChanged += (s, e) =>
                    {
                        if (e.NewItems != null)
                        {
                            foreach (StringItemViewModel item in e.NewItems)
                            {
                                item.WhenAnyValue(i => i.Value).Subscribe(_ => setting.OnValueChanged());
                            }
                        }
                        setting.OnValueChanged();
                    };

                    // Add existing items
                    foreach (var itemValue in list)
                    {
                        var itemViewModel = new StringItemViewModel(setting.Items) { Value = itemValue };
                        itemViewModel.WhenAnyValue(i => i.Value).Subscribe(_ => setting.OnValueChanged());
                        setting.Items.Add(itemViewModel);
                    }

                    settingViewModel = setting;
                }
                else
                {
                    continue; // Skip unsupported types
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
}