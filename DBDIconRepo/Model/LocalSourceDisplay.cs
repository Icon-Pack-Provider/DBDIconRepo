using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconInfo;
using IconInfo.Internal;
using System;
using System.IO;
using System.Security.Policy;
using System.Windows.Media.Imaging;

namespace DBDIconRepo.Model;

public partial class LocalSourceDisplay : ObservableObject, IDisplayItem
{
    public int DecodeWidth => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Width);
    public int DecodeHeight => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Height);

    private string type = "perk";

    [ObservableProperty]
    public IBasic? tooltip = null;

    public LocalSourceDisplay() { }
    public LocalSourceDisplay(string filePath)
    {
        URL = filePath;
        string urlparse = filePath.ToLower();

        if (urlparse.Contains("charselect_portrait"))
            type = "portrait";
        else if (urlparse.Contains("dailyritualicon"))
            type = "daily";
        else if (urlparse.Contains("emblemicon"))
            type = "emblem";
        else if (urlparse.Contains("iconfavors"))
            type = "offering";
        else if (urlparse.Contains("iconaddon"))
            type = "addon";
        else if (urlparse.Contains("iconitems"))
            type = "item";
        else if (urlparse.Contains("iconperks"))
            type = "perk";
        else if (urlparse.Contains("iconpowers"))
            type = "power";
        else if (urlparse.Contains("iconstatuseffects"))
            type = "status";
        else if (urlparse.Contains(".banner"))
            type = "banner";

        Tooltip = IconTypeIdentify.FromPath(filePath);

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var toDecode = IconResolutionScale.GetResolutionScale(type);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.DecodePixelWidth = Convert.ToInt32(toDecode.Width);
        bitmap.DecodePixelHeight = Convert.ToInt32(toDecode.Height);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        using BinaryReader br = new(fs);
        bitmap.StreamSource = new MemoryStream(br.ReadBytes((int)fs.Length));
        bitmap.EndInit();
        bitmap.Freeze();
        ImagePreviewSource = bitmap;
    }

    [ObservableProperty]
    string? _uRL;

    [ObservableProperty]
    BitmapImage imagePreviewSource = new();
}
