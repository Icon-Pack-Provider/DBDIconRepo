using CommunityToolkit.Mvvm.ComponentModel;
using IconInfo.Internal;

namespace DBDIconRepo.Model;

public partial class IconDisplay : OnlineSourceDisplay 
{
    public int DecodeWidth => System.Convert.ToInt32(IconResolutionScale.GetResolutionScale(Type).Width);
    public int DecodeHeight => System.Convert.ToInt32(IconResolutionScale.GetResolutionScale(Type).Height);

    [ObservableProperty] private string type = string.Empty;

    [ObservableProperty]
    public IBasic? tooltip = null;

    public IconDisplay() { }
    public IconDisplay(string url) : base(url) 
    {
        string urlparse = url.ToLower();
        if (urlparse.Contains("charselect_portrait"))
            Type = "portrait";
        else if (urlparse.Contains("dailyritualicon"))
            Type = "daily";
        else if (urlparse.Contains("emblemicon"))
            Type = "emblem";
        else if (urlparse.Contains("iconfavors"))
            Type = "offering";
        else if (urlparse.Contains("iconaddon"))
            Type = "addon";
        else if (urlparse.Contains("iconitems"))
            Type = "item";
        else if (urlparse.Contains("iconperks"))
            Type = "perk";
        else if (urlparse.Contains("iconpowers"))
            Type = "power";
        else if (urlparse.Contains("iconstatuseffects"))
            Type = "status";
    }
}
