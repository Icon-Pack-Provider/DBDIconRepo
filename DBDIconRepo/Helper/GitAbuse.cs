using DBDIconRepo.Model;
using IconPack.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Helper;

public static class GitAbuse
{
    public static async Task DownloadIndivisualItems(ObservableCollection<IPackSelectionItem>? installPick, Pack? info)
    {
        await Task.WhenAll(installPick.Where(pick => pick.IsSelected == true)
            .Select(picked =>
            {
                Messenger.Default.Send(
                    new IndetermineRepoProgressReportMessage(),
                    $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{info.Repository.Name}");
                return CacheOrGit.DownloadItem(info, picked.FullPath);
            }));
        /*foreach (IPackSelectionItem? selection in installPick)
        {
            if (!selection.IsSelected.HasValue)
                continue;
            else if (selection.IsSelected.Value)
            {
                //Download each icons
                await CacheOrGit.DownloadItem(info, selection.FullPath);
            }
        }*/
    }
}
