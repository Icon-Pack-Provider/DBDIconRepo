using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;

namespace IconPack.Model.Icon
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
