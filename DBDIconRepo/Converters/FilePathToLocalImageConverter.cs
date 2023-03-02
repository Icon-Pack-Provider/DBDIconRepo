using DBDIconRepo.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class FilePathToLocalImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            LocalSourceDisplay display = new LocalSourceDisplay(path);
            return display.ImagePreviewSource;
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
