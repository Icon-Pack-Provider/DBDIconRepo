using CommunityToolkit.Mvvm.ComponentModel;

namespace IconPack.Model
{
    public partial class Emblem : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
