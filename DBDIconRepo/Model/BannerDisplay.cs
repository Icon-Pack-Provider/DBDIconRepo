using System;

namespace DBDIconRepo.Model;

public class BannerDisplay : OnlineSourceDisplay
{
    public int DecodeWidth => Convert.ToInt32(IconResolutionScale.Banner.Width);
    public int DecodeHeight => Convert.ToInt32((int)IconResolutionScale.Banner.Height);

    public BannerDisplay() { }
    public BannerDisplay(string url) : base(url) { }
}
