using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using DBDIconRepo.ViewModel;
using IconPack;
using ModernWpf.Controls.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

public partial class DefaultPackView : Page
{
    //public HomeViewModel ViewModel { get; } = new HomeViewModel();

    public DefaultPackView()
    {
        InitializeComponent();
        this.Unloaded += UnregisterStuff;
        Messenger.Default.Register<DefaultPackView, RequestViewPackDetailMessage, string>(this,
            MessageToken.REQUESTVIEWPACKDETAIL, OpenPackDetailWindow);

        Task<IconPack.Model.Pack?[]> packFactory = Task.Run(async () =>
        {
            //https://github.com/Icon-Pack-Provider/Dead-by-daylight-Default-icons
            IconPack.Model.Pack? singularPac = null;
            var localRepoPathBD = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory, "Display", "Icon-Pack-Provider", "Dead-by-daylight-Default-icons", "pack.json");
            if (File.Exists(localRepoPathBD))
            {
                singularPac = Packs.GetLocalPack("Icon-Pack-Provider", "Dead-by-daylight-Default-icons");
                return new IconPack.Model.Pack?[] { singularPac };
            }

            Octokit.Repository defRepo = await OctokitService.Instance.GitHubClientInstance.Repository.Get("Icon-Pack-Provider", "Dead-by-daylight-Default-icons");
            IconPack.Model.Pack? onlineSingularPac = await Packs.GetPack(defRepo);
            return new IconPack.Model.Pack?[] { onlineSingularPac };
        });
        DataContext = new HomeViewModel(packFactory, new()
        {
            ShowFavoriteComponent = false
        });
    }

    private void OpenPackDetailWindow(DefaultPackView recipient, RequestViewPackDetailMessage message)
    {
        foreach (var window in Application.Current.Windows)
        {
            if (window is PackDetail pd)
            {
                if (pd.DataContext is PackDetailViewModel pdv && pdv.SelectedPack == message.Selected)
                {
                    pd.Hide();
                    pd.Show();
                    return;
                }
            }
        }

        PackDetail detail = new(message.Selected);
        detail.Show();
    }

    private void UnregisterStuff(object sender, RoutedEventArgs e)
    {
        Messenger.Default.UnregisterAll(this);
        ViewModel.UnregisterMessages();
    }

}