using DBDIconRepo.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class PerkSortByToBool : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PerkSortBy mode)
        {
            if (parameter is string str)
            {
                PerkSortBy compare = Enum.Parse<PerkSortBy>(str);
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
