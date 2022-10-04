using IconPack.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public interface IStar
{
    Task Initiallze();
    string ForOwner { get; set; }
    string ForName { get; set; }
    ObservableCollection<PackRepositoryInfo> AllStarred { get; set; }
    Task<bool> IsRepoStarred(PackRepositoryInfo info);
    Task Star(PackRepositoryInfo info);
    Task UnStar(PackRepositoryInfo info);
}
