using DBDIconRepo.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class EnumPreviewOptionToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PackPreviewOption option)
        {
            return (int)option;
        }
        return -1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selected)
        {
            return (PackPreviewOption)value;
        }
        return PackPreviewOption.UserDefined;
    }
}
