using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Model;
using System.Collections.Generic;
using System.Linq;

namespace DBDIconRepo.Model.History;
public partial class HistoryInstallPack : HistoryViewPack
{
    [ObservableProperty]
    //Keep only Root/Folder/[sub]/Filename
    private List<string?> installedIcons = new();

    public HistoryInstallPack(Pack? pack, IList<IPackSelectionItem> installed) : base(pack)
    {
        Action = HistoryType.Install;
        InstalledIcons = new(installed.Where(i => i.IsSelected == true).Select(i => i.Info?.File));
    }

    public HistoryInstallPack() { }
}