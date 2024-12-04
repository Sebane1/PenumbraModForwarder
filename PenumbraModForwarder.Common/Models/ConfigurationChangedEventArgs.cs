namespace PenumbraModForwarder.Common.Models;

public class ConfigurationChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
    public object NewValue { get; }

    public ConfigurationChangedEventArgs(string propertyName, object newValue)
    {
        PropertyName = propertyName;
        NewValue = newValue;
    }
}