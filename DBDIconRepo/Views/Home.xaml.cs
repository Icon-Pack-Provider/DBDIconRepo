using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.ViewModel;
using ModernWpf.Controls.Primitives;
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
}

public class IconPreviewTemplateSelector : DataTemplateSelector
{
    public DataTemplate IconDisplay { get; set; }
    public DataTemplate BannerDisplay { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Model.IconDisplay)
            return IconDisplay;
        else
            return BannerDisplay;
    }
}