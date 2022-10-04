using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.ViewModel;
using ModernWpf.Controls.Primitives;
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