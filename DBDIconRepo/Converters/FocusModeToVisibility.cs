using DBDIconRepo.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class FocusModeToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DetailFocusMode mode)
        {
            if (parameter is string compare)
            {
                DetailFocusMode cmode = Enum.Parse<DetailFocusMode>(compare);
                if (mode == cmode)
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
