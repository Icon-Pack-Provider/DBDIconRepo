using DBDIconRepo.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class PackStateToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PackState state)
        {
            if (parameter is string compare)
            {
                PackState cs = Enum.Parse<PackState>(compare);
                if (state == cs)
                    return Visibility.Visible;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
