using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.ViewModel;
using IconPack;
using ModernWpf.Controls.Primitives;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for FavoritePage.xaml
/// </summary>
public partial class FavoritePage : Page
{
    public FavoritePage()
    {
        InitializeComponent();
        this.Unloaded += UnregisterStuff;
        Messenger.Default.Register<FavoritePage, RequestViewPackDetailMessage, string>(this,
            MessageToken.REQUESTVIEWPACKDETAIL, OpenPackDetailWindow);

        Task<IconPack.Model.Pack?[]> packFactory = Task.Run(async () =>
        {
            var packs = await Packs.GetPacks();
            return packs.ToArray();
        });
        DataContext = new FavoriteViewModel(packFactory, new()
        {
            ShowFavoriteComponent = true
        });
    }

    private void OpenPackDetailWindow(FavoritePage recipient, RequestViewPackDetailMessage message)
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

    private void OpenAttatchedFlyout(object sender, RoutedEventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
    }
}