using CommunityToolkit.Mvvm.ComponentModel;

namespace IconPack.Model
{
    public partial class Perk : ObservableObject, IBasic
    {
#nullable enable
        [ObservableProperty]
        string? folder;
#nullable disable

        [ObservableProperty]
        string file;

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string owner;
    }
}
