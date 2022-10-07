using DBDIconRepo.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

internal class ResolutionFromScaleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double scale && parameter is string type)
        {
            var res = IconResolutionScale.GetResolutionScale(type);
            return $"{res.Width:00} × {res.Height:00}";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
