using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Internal;

namespace IconPack.Model
{
    public partial class Offering : ObservableObject, IBasic, IFolder
    {
#nullable enable
        [ObservableProperty]
        string? folder;
#nullable disable

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
