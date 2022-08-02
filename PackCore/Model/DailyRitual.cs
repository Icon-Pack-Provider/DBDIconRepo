using CommunityToolkit.Mvvm.ComponentModel;

namespace IconPack.Model
{
    public partial class DailyRitual : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
