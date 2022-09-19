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
        if (value is byte[] image)
        {
            bool isBanner = parameter is not null && parameter.ToString() == "banner";
            (int w, int h) decode =
                (isBanner ? Setting.Instance.BannerDecodeWidth : Setting.Instance.IconPreviewDecodeWidth,
                isBanner ? Setting.Instance.BannerDecodeHeight : Setting.Instance.IconPreviewDecodeHeight);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = decode.w;
            bitmap.DecodePixelHeight = decode.h;
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
