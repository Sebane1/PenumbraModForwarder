using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class SettingViewModel : ViewModelBase
{
    private object _value;

    public string DisplayName { get; set; }
    public string GroupName { get; set; }

    public virtual object Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            OnValueChanged();
        }
    }

    // Action to update the configuration model when the value changes
    public Action<object> ValueChangedAction { get; set; }

    public virtual void OnValueChanged()
    {
        ValueChangedAction?.Invoke(Value);
    }
}

public class BooleanSettingViewModel : SettingViewModel
{
    public new bool Value
    {
        get => base.Value != null && (bool)base.Value;
        set => base.Value = value;
    }
}

public class StringSettingViewModel : SettingViewModel
{
    public new string Value
    {
        get => (string)base.Value ?? string.Empty;
        set => base.Value = value;
    }
}


public class NumberSettingViewModel<T> : SettingViewModel, IDataErrorInfo where T : struct, IComparable, IFormattable
{
    private string _input;
    private string _error;

    public NumberSettingViewModel()
    {
        // Initialize Input with the string representation of Value
        _input = Value.ToString(null, CultureInfo.InvariantCulture);
    }

    public new T Value
    {
        get => base.Value != null ? (T)base.Value : default;
        set
        {
            base.Value = value;

            // Update _input directly to prevent recursive calls
            var newInput = value.ToString(null, CultureInfo.InvariantCulture);
            if (_input != newInput)
            {
                _input = newInput;
                this.RaisePropertyChanged(nameof(Input));
            }
        }
    }

    public string Input
    {
        get => _input;
        set
        {
            this.RaiseAndSetIfChanged(ref _input, value);
            if (TryParseInput(value, out T parsedValue))
            {
                // Update Value if it has changed
                if (!EqualityComparer<T>.Default.Equals(Value, parsedValue))
                {
                    base.Value = parsedValue;
                    this.RaisePropertyChanged(nameof(Value));
                }
                _error = null;
            }
            else
            {
                _error = "Invalid number format.";
            }
            // Notify that Error has changed
            this.RaisePropertyChanged(nameof(Error));
            this.RaisePropertyChanged("Item[]");
        }
    }

    public string Error => _error;

    public string this[string columnName] => _error;

    private bool TryParseInput(string input, out T result)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null && !string.IsNullOrWhiteSpace(input))
        {
            try
            {
                result = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
                return true;
            }
            catch
            {
                // Handle parsing exception if necessary
            }
        }
        result = default;
        return false;
    }
}

public class IntSettingViewModel : NumberSettingViewModel<int>
{
    
}

public class DoubleSettingViewModel : NumberSettingViewModel<double>
{
    
}

public class DecimalSettingViewModel : NumberSettingViewModel<decimal>
{
    
}

public class FloatSettingViewModel : NumberSettingViewModel<float>
{
    
}

public class StringItemViewModel : SettingViewModel
{
    private string _value;
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

    public StringItemViewModel(ObservableCollection<StringItemViewModel> parentCollection)
    {
        RemoveCommand = ReactiveCommand.Create(() =>
        {
            parentCollection.Remove(this);
        });
    }
}

public class ListStringSettingViewModel : SettingViewModel
{
    public ListStringSettingViewModel()
    {
        Value = new ObservableCollection<StringItemViewModel>();
        AddItemCommand = ReactiveCommand.Create(AddItem);

        // Subscribe to collection changes to trigger ValueChangedAction
        Value.CollectionChanged += (s, e) => OnValueChanged();
    }

    public new ObservableCollection<StringItemViewModel> Value
    {
        get => base.Value as ObservableCollection<StringItemViewModel> ?? new ObservableCollection<StringItemViewModel>();
        set
        {
            base.Value = value;
            this.RaisePropertyChanged();
            OnValueChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> AddItemCommand { get; }

    private void AddItem()
    {
        var item = new StringItemViewModel(Value) { Value = string.Empty };
        Value.Add(item);
    }

    public override void OnValueChanged()
    {
        var stringList = Value.Select(item => item.Value).ToList();
        ValueChangedAction?.Invoke(stringList);
    }
}