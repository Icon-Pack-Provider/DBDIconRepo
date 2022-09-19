using CommunityToolkit.Mvvm.ComponentModel;

namespace DBDIconRepo.Model;

public partial class OnlineSourceDisplay : ObservableObject, IDisplayItem
{
    public OnlineSourceDisplay() { }

    public OnlineSourceDisplay(string url) { URL = url; }

    [ObservableProperty]
    string? _uRL;
}
