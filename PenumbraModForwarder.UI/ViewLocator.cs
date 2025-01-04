using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using PenumbraModForwarder.UI.ViewModels;

namespace PenumbraModForwarder.UI;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Control> _viewCache = new();

    public Control Build(object data)
    {
        var viewModelType = data.GetType();
        var viewName = viewModelType.FullName!.Replace("ViewModel", "View");
        var viewType = Type.GetType(viewName);

        if (viewType != null)
        {
            if (!_viewCache.TryGetValue(viewType, out var control))
            {
                control = (Control)Activator.CreateInstance(viewType)!;
                _viewCache[viewType] = control;
            }
            return control;
        }

        return new TextBlock { Text = $"Not Found: {viewName}" };
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}