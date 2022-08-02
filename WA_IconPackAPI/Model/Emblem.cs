using CommunityToolkit.Mvvm.ComponentModel;
using IconPackAPI.Internal;

namespace IconPackAPI.Model
{
    public partial class Emblem : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
