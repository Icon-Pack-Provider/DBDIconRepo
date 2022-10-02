using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class AnonymousIconRandomizer : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int rand = new Random().Next(0, 10);
        switch (rand)
        {
            case 2:
            case 3:
                return '\uEE57';
            case 4:
            case 5:
                return '\uE11B'; //Help
            case 6:
            case 7:
                return '\uE8C9'; 
            case 8:
            case 9:
                return '\uE1A6'; //Other user
            case 10:
                return '\uF1AD'; //Ninja cat
            case 0:
            case 1:
            default:
                return '\uE99A';
        }

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
