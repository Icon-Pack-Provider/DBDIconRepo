using System;

namespace DBDIconRepo.Model;

public class IconDisplay : OnlineSourceDisplay 
{
    public int DecodeWidth => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Width);
    public int DecodeHeight => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Height);

    private string type = "perk";

    public IconDisplay() { }
    public IconDisplay(string url) : base(url) 
    {
        string urlparse = url.ToLower();
        if (urlparse.Contains("charselect_portrait"))
            type = "portrait";
        else if (urlparse.Contains("dailyritualicon"))
            type = "daily";
        else if (urlparse.Contains("emblemIcon"))
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
    }
}
