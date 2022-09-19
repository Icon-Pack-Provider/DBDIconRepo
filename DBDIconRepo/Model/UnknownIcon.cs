using CommunityToolkit.Mvvm.ComponentModel;
using IconInfo.Internal;

namespace DBDIconRepo.Model;

public partial class UnknownIcon : ObservableObject, IBasic
{
    public UnknownIcon() { }
    public UnknownIcon(string path)
    {
        //this.File = path.NameOnly();
        //this.Name = path.NameOnly();
    }
    [ObservableProperty]
    private string file;

    [ObservableProperty]
    private string name;
}
