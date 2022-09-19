using DBDIconRepo.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class FocusModeToGridLength : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DetailFocusMode dfm)
        {
            if (dfm == DetailFocusMode.Overview)
                return GridLength.Auto;
            if (parameter is string set)
            {
                DetailFocusMode compare = Enum.Parse<DetailFocusMode>(set);
                if (compare == dfm)
                    return new GridLength(1, GridUnitType.Star);
                return new GridLength(0, GridUnitType.Pixel);
            }
        }
        return GridLength.Auto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
