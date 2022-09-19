using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class URLtoAbsoluteURI : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string url)
        {
            return new Uri(url, UriKind.Absolute);
        }
        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
