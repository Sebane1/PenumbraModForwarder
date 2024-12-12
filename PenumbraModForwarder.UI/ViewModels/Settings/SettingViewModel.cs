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
    public bool TypedValue
    {
        get => Value != null && (bool)Value;
        set => Value = value;
    }
}

public class StringSettingViewModel : SettingViewModel
{
    public string TypedValue
    {
        get => Value as string ?? string.Empty;
        set => Value = value;
    }
}

public class NumberSettingViewModel<T> : SettingViewModel, IDataErrorInfo
    where T : struct, IComparable, IFormattable
{
    private string _input;
    private string _error;

    public NumberSettingViewModel()
    {
        _input = Value?.ToString() ?? string.Empty;
    }

    public T TypedValue
    {
        get => Value != null ? (T)Value : default;
        set => Value = value;
    }

    public string Input
    {
        get => _input;
        set
        {
            this.RaiseAndSetIfChanged(ref _input, value);
            if (TryParseInput(value, out T parsedValue))
            {
                if (!EqualityComparer<T>.Default.Equals(TypedValue, parsedValue))
                {
                    TypedValue = parsedValue;
                }
                _error = null;
            }
            else
            {
                _error = "Invalid number format.";
            }
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

public class IntSettingViewModel : NumberSettingViewModel<int> { }

public class DoubleSettingViewModel : NumberSettingViewModel<double> { }

public class DecimalSettingViewModel : NumberSettingViewModel<decimal> { }

public class FloatSettingViewModel : NumberSettingViewModel<float> { }

public class StringItemViewModel : ViewModelBase
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
        Items = new ObservableCollection<StringItemViewModel>();
        AddItemCommand = ReactiveCommand.Create(AddItem);

        // Subscribe to collection changes to trigger ValueChangedAction
        Items.CollectionChanged += (s, e) => OnValueChanged();

        // Subscribe to property changes of items to trigger ValueChangedAction
        foreach (var item in Items)
        {
            item.WhenAnyValue(i => i.Value).Subscribe(_ => OnValueChanged());
        }
    }

    public ObservableCollection<StringItemViewModel> Items
    {
        get => Value as ObservableCollection<StringItemViewModel>;
        set
        {
            Value = value;
            this.RaisePropertyChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> AddItemCommand { get; }

    private void AddItem()
    {
        var item = new StringItemViewModel(Items) { Value = string.Empty };
        item.WhenAnyValue(i => i.Value).Subscribe(_ => OnValueChanged());
        Items.Add(item);
    }

    public override void OnValueChanged()
    {
        // Convert the collection of StringItemViewModel to List<string>
        var stringList = Items.Select(item => item.Value).ToList();
        ValueChangedAction?.Invoke(stringList);
    }
}