using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;

namespace IconPack.Model.Icon
{
    public partial class Emblem : ObservableObject, IBasic
    {
        [ObservableProperty]
        string file;

        [ObservableProperty]
        string name;
    }
}
