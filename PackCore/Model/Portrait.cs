using CommunityToolkit.Mvvm.ComponentModel;

namespace IconPack.Model
{
    public partial class Portrait : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
