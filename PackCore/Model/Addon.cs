using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Internal;

namespace IconPack.Model
{
    public partial class Addon : ObservableObject, IBasic, IFolder
    {
#nullable enable
        [ObservableProperty]
        string? folder;
#nullable disable

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;

        [ObservableProperty]
        string _for;

        [ObservableProperty]
        string owner;
    }
}
