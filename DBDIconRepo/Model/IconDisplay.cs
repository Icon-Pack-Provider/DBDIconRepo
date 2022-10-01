namespace DBDIconRepo.Model;

public class IconDisplay : OnlineSourceDisplay 
{
    public int DecodeWidth => SettingManager.Instance.IconPreviewDecodeWidth;
    public int DecodeHeight => SettingManager.Instance.IconPreviewDecodeHeight;

    public IconDisplay() { }
    public IconDisplay(string url) : base(url) { }
}
