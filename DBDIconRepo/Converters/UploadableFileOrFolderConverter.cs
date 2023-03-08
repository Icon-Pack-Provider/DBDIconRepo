using DBDIconRepo.Model.Uploadable;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

/// <summary>
/// Return true if its an UploadableFolder, return false if its an UploadableFile
/// </summary>
public class UploadableFileOrFolderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is UploadableFolder)
            return true;
        else if (value is UploadableFile)
            return false;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
