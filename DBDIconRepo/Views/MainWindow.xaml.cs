using DBDIconRepo.Dialog;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using DBDIconRepo.ViewModel;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Page
{
    public HomeViewModel ViewModel { get; } = new HomeViewModel();

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += LoadPacklist;
        this.Unloaded += UnregisterStuff;
        DataContext = ViewModel;
        Messenger.Default.Register<MainWindow, RequestViewPackDetailMessage, string>(this,
            MessageToken.REQUESTVIEWPACKDETAIL, OpenPackDetailWindow);
    }
    

    private void OpenPackDetailWindow(MainWindow recipient, RequestViewPackDetailMessage message)
    {
        foreach (var window in Application.Current.Windows)
        {
            if (window is PackDetail pd)
            {
                if (pd.ViewModel.SelectedPack == message.Selected)
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

    private void LoadPacklist(object sender, RoutedEventArgs e)
    {
        ViewModel.InitializeViewModel();
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
