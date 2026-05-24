using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShopERP.Rebuild.Desktop.Converters;

public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasValue = false;

        if (value is string str)
        {
            hasValue = !string.IsNullOrWhiteSpace(str);
        }
        else if (value is int i)
        {
            hasValue = i > 0;
        }
        else if (value is bool b)
        {
            hasValue = b;
        }
        else
        {
            hasValue = value != null;
        }

        bool inverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
        
        if (inverse)
        {
            return hasValue ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }
}
