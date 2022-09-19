using DBDIconRepo.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

internal class FocusModeToMaxLength : IValueConverter
{
    /// <summary>
    /// Convert DetailFocusMode to Length comparison
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Must contains 3 string with | as separator
    /// Overview|0|150
    /// </param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DetailFocusMode mode)
        {
            if (parameter is string otherMode)
            {
                string[] target = parameter.ToString().Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (target.Length < 1)
                    return 150;
                var compare = Enum.Parse<DetailFocusMode>(target[0]);
                int first = int.Parse(target[1]);
                int second = int.Parse(target[2]);
                if (second == -1)
                    second = int.MaxValue;
                if (compare == mode)
                    return first;
                return second;
            }
        }
        return 150;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
