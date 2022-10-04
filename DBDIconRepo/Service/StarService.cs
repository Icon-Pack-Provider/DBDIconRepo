using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using IconPack.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Service;

public partial class StarService : ObservableObject
{
    public StarService()
    {
        OctokitService.Instance.PropertyChanged += MonitorAnonymousStatus;
    }

    private void MonitorAnonymousStatus(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OctokitService.IsAnonymous))
        {
            //Switch base
            InitializeStarService();
        }
    }

    [ObservableProperty]
    private IStar? baseService = null;

    private static StarService _instance = new();
    public static StarService Instance => _instance;

    public void InitializeStarService()
    {
        List<PackRepositoryInfo> preSwitch = BaseService is null ? new() : new(BaseService.AllStarred);
        List<PackRepositoryInfo> postSwitch = new();
        if (OctokitService.Instance.IsAnonymous)
            BaseService = new LocalStarHelper();
        else //Check permission?
            BaseService = new OnlineStarHelper();
        Task.Run(async () =>
        await BaseService.Initiallze())
            .Await(() =>
            {
                OnPropertyChanged(nameof(BaseService));
                postSwitch = new(BaseService.AllStarred);
                Messenger.Default.Send(new MassRepoStarChanged(preSwitch, postSwitch), MessageToken.MassRepoStarChangedToken);
            });
    }
}
