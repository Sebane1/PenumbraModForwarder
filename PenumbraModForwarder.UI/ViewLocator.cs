using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.ViewModels.Settings;

namespace PenumbraModForwarder.UI;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        // For settings-related view models, let the DataTemplates handle them
        if (data is SettingViewModel || data is SettingGroupViewModel)
        {
            return null!;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = $"Not Found: {name}" };
    }

    public bool Match(object data)
    {
        // Don't match any settings-related ViewModels
        if (data is SettingViewModel || data is SettingGroupViewModel)
        {
            return false;
        }

        return data is ViewModelBase;
    }
}