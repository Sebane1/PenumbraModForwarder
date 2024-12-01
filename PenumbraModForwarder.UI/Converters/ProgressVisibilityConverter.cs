using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PenumbraModForwarder.UI.Converters;

public class ProgressVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            return progress >= 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}