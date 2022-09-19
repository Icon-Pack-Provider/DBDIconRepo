using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class NullableStringPropertyDisplayConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? "N/A" : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
