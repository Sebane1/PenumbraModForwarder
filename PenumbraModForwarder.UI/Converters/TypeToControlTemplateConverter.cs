using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Templates;
using PenumbraModForwarder.UI.Helpers;

namespace PenumbraModForwarder.UI.Converters;

public class TypeToControlTemplateConverter : IValueConverter
{
    public DataTemplate BooleanTemplate { get; set; }
    public DataTemplate StringTemplate { get; set; }
    public DataTemplate IntegerTemplate { get; set; }
    public DataTemplate PathTemplate { get; set; }
    public DataTemplate MultiPathTemplate { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ConfigurationPropertyDescriptor descriptor)
        {
            var type = descriptor.PropertyInfo.PropertyType;
            var displayName = descriptor.DisplayName;

            if (type == typeof(bool))
                return BooleanTemplate;
            if (type == typeof(string) && displayName.Contains("Path", StringComparison.OrdinalIgnoreCase))
                return PathTemplate;
            if (type == typeof(List<string>))
                return MultiPathTemplate;
            if (type == typeof(string))
                return StringTemplate;
            if (type == typeof(int))
                return IntegerTemplate;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}