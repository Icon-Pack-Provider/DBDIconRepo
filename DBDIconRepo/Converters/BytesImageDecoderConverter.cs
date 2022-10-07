using DBDIconRepo.Model;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DBDIconRepo.Converters;

public class BytesImageDecoderConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] image && parameter is string type)
        {
            var toDecode = IconResolutionScale.GetResolutionScale(type);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = System.Convert.ToInt32(toDecode.Width);
            bitmap.DecodePixelHeight = System.Convert.ToInt32(toDecode.Height);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(image);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
