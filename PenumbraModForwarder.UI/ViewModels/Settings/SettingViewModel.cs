using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reactive;
using System.Reflection;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class SettingViewModel : ReactiveObject
{
    public string DisplayName { get; set; }
    public string GroupName { get; set; }

    // Properties to assist in saving settings
    public Type ModelType { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
}

public class BooleanSettingViewModel : SettingViewModel
{
    private bool _typedValue;

    public bool TypedValue
    {
        get => _typedValue;
        set => this.RaiseAndSetIfChanged(ref _typedValue, value);
    }
}

public class StringSettingViewModel : SettingViewModel
{
    private string _typedValue = string.Empty;

    public string TypedValue
    {
        get => _typedValue;
        set => this.RaiseAndSetIfChanged(ref _typedValue, value ?? string.Empty);
    }
}

public class NumberSettingViewModel<T> : SettingViewModel, IDataErrorInfo
    where T : struct, IComparable, IFormattable
{
    private T _typedValue;
    private string _input;
    private string _error;

    public NumberSettingViewModel()
    {
        // Initialize Input based on TypedValue
        _input = _typedValue.ToString(null, CultureInfo.InvariantCulture);

        // Handle changes to Input
        this.WhenAnyValue(vm => vm.Input)
            .Subscribe(newValue =>
            {
                if (TryParseInput(newValue, out T parsedValue))
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
            });
    }

    public T TypedValue
    {
        get => _typedValue;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_typedValue, value))
            {
                this.RaiseAndSetIfChanged(ref _typedValue, value);
                // Update Input when TypedValue changes
                Input = _typedValue.ToString(null, CultureInfo.InvariantCulture);
            }
        }
    }

    public string Input
    {
        get => _input;
        set => this.RaiseAndSetIfChanged(ref _input, value);
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

public class StringItemViewModel : ReactiveObject
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
    private ObservableCollection<StringItemViewModel> _items;

    public ListStringSettingViewModel()
    {
        _items = new ObservableCollection<StringItemViewModel>();
        AddItemCommand = ReactiveCommand.Create(AddItem);
    }

    public ObservableCollection<StringItemViewModel> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }

    public ReactiveCommand<Unit, Unit> AddItemCommand { get; }

    private void AddItem()
    {
        var item = new StringItemViewModel(Items)
        {
            Value = string.Empty
        };
        Items.Add(item);
    }
}