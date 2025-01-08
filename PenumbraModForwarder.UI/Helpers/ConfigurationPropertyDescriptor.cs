using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using NLog;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Helpers;

public class ConfigurationPropertyDescriptor : ReactiveObject
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string GroupName { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public object ModelInstance { get; set; }
    public ConfigurationPropertyDescriptor ParentDescriptor { get; set; }

    private object _value;
    public object Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            UpdateModelValue(value);

            // If we're dealing with a List<string>, update the path items
            if (PropertyInfo?.PropertyType == typeof(List<string>))
            {
                UpdatePathItems();
            }
        }
    }

    public ICommand BrowseCommand { get; set; }

    public ObservableCollection<PathItemViewModel> PathItems { get; } = new();

    private void UpdatePathItems()
    {
        PathItems.Clear();

        if (Value is List<string> paths)
        {
            var uniquePaths = paths.Distinct().ToList();
            foreach (var path in uniquePaths)
            {
                var pathItem = new PathItemViewModel(path, this);
                PathItems.Add(pathItem);
            }
        }
    }

    internal void RemovePath(PathItemViewModel pathItem)
    {
        if (Value is List<string> paths && paths.Contains(pathItem.Path))
        {
            var newPaths = new List<string>(paths);
            newPaths.Remove(pathItem.Path);
            Value = newPaths;
            PathItems.Remove(pathItem);
        }
    }

    private void UpdateModelValue(object value)
    {
        try
        {
            object convertedValue;

            if (PropertyInfo?.PropertyType == typeof(int) && value is decimal decimalValue)
            {
                convertedValue = Convert.ToInt32(decimalValue);
            }
            else if (PropertyInfo?.PropertyType == typeof(string))
            {
                convertedValue = value?.ToString();
            }
            else if (PropertyInfo?.PropertyType == typeof(List<string>) && value is IEnumerable<string> enumerable)
            {
                convertedValue = new List<string>(enumerable);
            }
            else
            {
                convertedValue = Convert.ChangeType(value, PropertyInfo?.PropertyType ?? typeof(object));
            }

            PropertyInfo?.SetValue(ModelInstance, convertedValue);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update value for property {Name}", PropertyInfo?.Name);
        }
    }
}