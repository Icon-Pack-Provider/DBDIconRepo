using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class TextSwitchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //Input: ["!" for invert][iconA];[iconB]
        //True: [iconA] False: [iconB]
        //False [iconA] True   [iconB]
        if (parameter is string text && value is bool input)
        {
            bool isInvert = false;
            if (text.StartsWith('!'))
                isInvert = true;
            if (isInvert)
                return input ? text[3] : text[1];
            else
                return input ? text[0] : text[2];
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
