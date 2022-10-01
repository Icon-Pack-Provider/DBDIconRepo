namespace DBDIconRepo.Model;

public class BannerDisplay : OnlineSourceDisplay
{
    public int DecodeWidth => SettingManager.Instance.BannerDecodeWidth;
    public int DecodeHeight => SettingManager.Instance.BannerDecodeHeight;

    public BannerDisplay() { }
    public BannerDisplay(string url) : base(url) { }
}
