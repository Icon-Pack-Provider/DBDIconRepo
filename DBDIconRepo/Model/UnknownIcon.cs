using CommunityToolkit.Mvvm.ComponentModel;
using IconInfo.Internal;
using System.IO;

namespace DBDIconRepo.Model;

public partial class UnknownIcon : ObservableObject, IBasic, IFolder
{
    public UnknownIcon() { }
    public UnknownIcon(string path)
    {
        this.Folder = Path.GetDirectoryName(path);
        this.File = Path.GetFileNameWithoutExtension(path);
        this.Name = Path.GetFileNameWithoutExtension(path);
    }
    [ObservableProperty]
    private string file;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string? folder;
}
