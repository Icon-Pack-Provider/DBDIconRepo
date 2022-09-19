using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Model;

namespace DBDIconRepo.Model;

public class GenericItemInfo : ObservableObject, IBaseItemInfo
{
    string? _name;
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public GenericItemInfo() { }
    public GenericItemInfo(string name) { Name = name; }
}
