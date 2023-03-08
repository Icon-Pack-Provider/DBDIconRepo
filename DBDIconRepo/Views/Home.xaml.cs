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

public partial class Home : Page
{
    //public HomeViewModel ViewModel { get; } = new HomeViewModel();

    public Home()
    {
        InitializeComponent();
        this.Unloaded += UnregisterStuff;
        Messenger.Default.Register<Home, RequestViewPackDetailMessage, string>(this,
            MessageToken.REQUESTVIEWPACKDETAIL, OpenPackDetailWindow);

        Task<IconPack.Model.Pack?[]> packFactory = Task.Run(async () =>
        {
            var packs = await Packs.GetPacks();
            return packs.ToArray();
        });
        DataContext = new HomeViewModel(packFactory, new()
        {
            ShowFavoriteComponent = true
        });
    }

    private void OpenPackDetailWindow(Home recipient, RequestViewPackDetailMessage message)
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

public class IconPreviewTemplateSelector : DataTemplateSelector
{
    public DataTemplate IconDisplay { get; set; }
    public DataTemplate BannerDisplay { get; set; }
    public DataTemplate LocalIconDisplay { get; set; }
    public DataTemplate LocalBannerDisplay { get; set; }
    public DataTemplate PlaceholderBannerDisplay { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Model.LocalSourceDisplay local)
        {
            if (local.URL.Contains(".banner"))
                return LocalBannerDisplay;
            return LocalIconDisplay;
        }
        else if (item is Model.PlaceholderSourceDisplay)
        {
            return PlaceholderBannerDisplay;
        }
        if (item is Model.IconDisplay)
            return IconDisplay;
        else
            return BannerDisplay;
    }
}