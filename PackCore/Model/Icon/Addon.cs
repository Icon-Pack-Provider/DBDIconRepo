using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;
using IconPack.Internal;

namespace IconPack.Model.Icon
{
    public partial class Addon : ObservableObject, IBasic, IFolder
    {
#nullable enable
        [ObservableProperty]
        string? folder;
#nullable disable

        [ObservableProperty]
        string file;

        //Power name
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string _for;

        [ObservableProperty]
        string owner;
    }
}
