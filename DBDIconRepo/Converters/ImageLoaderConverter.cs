using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using NetVips;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Converters;

/// <summary>
/// Load image via NetVips and return a BitmapSource
/// Input path must be local file
/// </summary>
public class ImageLoaderConverter : IValueConverter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Parameter for custom decode size
    /// banner > Banner encoding size
    /// icon > Icon encoding size</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string localFilePath = string.Empty;
        Size? resize = null;
        double? resizePercentage = null;
        if (value is null)
            return null;
        if (value is string path)
        {
            if (string.IsNullOrEmpty(path)) 
            {
                //loading icon
                return SendLoadingIcon();
            }
            localFilePath = path;
            if (OctokitService.Instance.IsAnonymous)
            {
                return JustSendURI(path);
            }
        }
        if (value is OnlineSourceDisplay src) 
        {
            if (string.IsNullOrEmpty(src.LocalizedURL))
            {
                if (OctokitService.Instance.IsAnonymous)
                    return JustSendURI(src.LocalizedURL);
                else
                {
                    Messenger.Default.Send(new AttemptReloadIconMessage(src.URL), MessageToken.AttemptReloadIconMessage);
                    return SendLoadingIcon();
                }
            }
            else if (src.LocalizedURL == "TIMEOUT")
            {
                //Just use URL
                return JustSendURI(src.URL);
            }
            localFilePath = src.LocalizedURL;
        }
        if (parameter is string param)
        {
            resize = IconResolutionScale.GetResolutionScale(param);
            resizePercentage = IconResolutionScale.GetScale(param);
        }
        if (value is IconDisplay iconScale)
        {
            resize = new Size(iconScale.DecodeWidth, iconScale.DecodeHeight);
            resizePercentage = IconResolutionScale.GetScale(iconScale.Type);
        }
        if (value is BannerDisplay bannerScale)
        {
            resize = IconResolutionScale.GetResolutionScale("banner");
            resizePercentage = IconResolutionScale.GetScale("banner");
        }
        
        using var image = Image.NewFromFile(localFilePath);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        if (resize.HasValue && resizePercentage.HasValue)
        {
            image.Resize(resizePercentage.Value);
            bitmap.DecodePixelWidth = (int)resize.Value.Width;
            bitmap.DecodePixelHeight = (int)resize.Value.Height;
        }
        var bytes = image.PngsaveBuffer();

        bitmap.StreamSource = new MemoryStream(bytes);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        if (bitmap.CanFreeze)
            bitmap.Freeze();
        return bitmap;
    }

    private BitmapImage JustSendURI(string url)
    {
        var giveup = new BitmapImage();
        giveup.BeginInit();
        giveup.CacheOption = BitmapCacheOption.OnLoad;
        giveup.UriSource = new(url, UriKind.Absolute);
        giveup.EndInit();
        if (giveup.CanFreeze)
            giveup.Freeze();
        return giveup;
    }

    private BitmapImage SendLoadingIcon()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(DBDIconRepo)}.Resources.loadingicon.png");
        using var image = Image.NewFromStream(stream);
        var bytes = image.PngsaveBuffer();

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = new MemoryStream(bytes);
        bitmap.EndInit();
        if (bitmap.CanFreeze)
            bitmap.Freeze();
        return bitmap;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}