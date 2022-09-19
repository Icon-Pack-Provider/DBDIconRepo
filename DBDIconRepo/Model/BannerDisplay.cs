namespace DBDIconRepo.Model;

public class BannerDisplay : OnlineSourceDisplay
{
    public int DecodeWidth => Setting.Instance.BannerDecodeWidth;
    public int DecodeHeight => Setting.Instance.BannerDecodeHeight;

    public BannerDisplay() { }
    public BannerDisplay(string url) : base(url) { }
}
