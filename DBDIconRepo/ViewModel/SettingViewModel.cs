using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;

namespace DBDIconRepo.ViewModel;

public partial class SettingViewModel : ObservableObject
{
    public Setting Config => SettingManager.Instance;
}
