using CommunityToolkit.Mvvm.ComponentModel;

namespace DBDIconRepo.Model;

public partial class PlaceholderSourceDisplay : ObservableObject, IDisplayItem
{
    [ObservableProperty]
    private string? _uRL;
}
