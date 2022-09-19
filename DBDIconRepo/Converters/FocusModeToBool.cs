using DBDIconRepo.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class FocusModeToBool : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DetailFocusMode mode)
        {
            if (parameter is string str)
            {
                DetailFocusMode compare = Enum.Parse<DetailFocusMode>(str);
                if (mode == compare)
                    return true;
            }
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
