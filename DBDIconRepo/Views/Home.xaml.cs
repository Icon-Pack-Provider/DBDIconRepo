using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.ViewModel;
using IconPack;
using ModernWpf.Controls.Primitives;
using System.Diagnostics;
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

    private void SizeTrigger(object sender, SizeChangedEventArgs e)
    {
        /* 86   376  //Sort
         * 108  462 //Filter
         * 110  570 //View
         * 140  680 //Download*/

        sortButtonCMD.LabelPosition = e.NewSize.Width >= 660 ? ModernWpf.Controls.CommandBarLabelPosition.Default :
            ModernWpf.Controls.CommandBarLabelPosition.Collapsed;
        filterButtonCMD.LabelPosition = e.NewSize.Width >= 690 ? ModernWpf.Controls.CommandBarLabelPosition.Default :
            ModernWpf.Controls.CommandBarLabelPosition.Collapsed;
        layoutButtonCMD.LabelPosition = e.NewSize.Width >= 740 ? ModernWpf.Controls.CommandBarLabelPosition.Default :
            ModernWpf.Controls.CommandBarLabelPosition.Collapsed;
        downloadButtonCMD.LabelPosition = e.NewSize.Width >= 820 ? ModernWpf.Controls.CommandBarLabelPosition.Default :
            ModernWpf.Controls.CommandBarLabelPosition.Collapsed;     
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