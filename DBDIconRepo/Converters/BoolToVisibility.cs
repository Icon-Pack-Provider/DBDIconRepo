using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

internal class BoolToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool input)
        {
            return input ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility input)
        {
            return input == Visibility.Visible ? true : false;
        }
        return false;
    }
}
