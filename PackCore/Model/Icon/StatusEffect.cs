using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;

namespace IconPack.Model.Icon
{
    public partial class StatusEffect : ObservableObject, IBasic
    {
        [ObservableProperty]
        string file;

        //Power name
        [ObservableProperty]
        string name;
    }
}
