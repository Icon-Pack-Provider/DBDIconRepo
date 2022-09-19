using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class ShowSpecificTextIfNullConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return parameter.ToString();
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
