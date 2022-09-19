namespace DBDIconRepo.Model;

public class IconDisplay : OnlineSourceDisplay 
{
    public int DecodeWidth => Setting.Instance.IconPreviewDecodeWidth;
    public int DecodeHeight => Setting.Instance.IconPreviewDecodeHeight;

    public IconDisplay() { }
    public IconDisplay(string url) : base(url) { }
}
