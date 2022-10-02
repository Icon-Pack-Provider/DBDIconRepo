using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconInfo.Internal;

namespace DBDIconRepo.Model;

public interface IPackSelectionItem
{
    string? FullPath { get; set; }
    string? FilePath { get; }
    bool? IsSelected { get; set; }
    IBasic? Info { get; set; }
}

public partial class PackSelectionFile : ObservableObject, IPackSelectionItem
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilePath))]
    string? fullPath;
    public string FilePath
    {
        get
        {
            if (FullPath is null)
                return "";
            return FullPath.Replace('/', '\\');
        }
    }

    [ObservableProperty]
    bool? isSelected = true;

    [ObservableProperty]
    IBasic? info;

    public PackSelectionFile() { }

    public PackSelectionFile(string path)
    {
        Info = IconTypeIdentify.FromPath(path);
        FullPath = path;
    }
}
