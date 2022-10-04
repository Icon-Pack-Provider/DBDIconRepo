using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Preview;
using IconInfo.Icon;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for Home.xaml
/// </summary>
public partial class Home : Window
{
    private static bool attempt = false;
    public Home()
    {
        InitializeComponent();
        Messenger.Default.Register<Home, SwitchToOtherPageMessage, string>(this, MessageToken.RequestMainPageChage,
            SwitchPageHandler);
        //Force inducing a few seconds of eye seizure to fix color issue
        Task.Run(async () =>
        {
            if (attempt)
                return;
            attempt = true;
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                ModernWpf.ThemeManager.SetRequestedTheme(this, ModernWpf.ElementTheme.Light);
                await Task.Delay(50);
                ModernWpf.ThemeManager.SetRequestedTheme(this, ModernWpf.ElementTheme.Dark);
                await Task.Delay(50);
                ModernWpf.ThemeManager.SetRequestedTheme(this, ModernWpf.ElementTheme.Light);
                await Task.Delay(50);
                ModernWpf.ThemeManager.SetRequestedTheme(this, ModernWpf.ElementTheme.Dark);
                await Task.Delay(50);
                ModernWpf.ThemeManager.SetRequestedTheme(this, ModernWpf.ElementTheme.Default);
            }, System.Windows.Threading.DispatcherPriority.Send);
        }).Await(() =>
        {

        });
    }

    private void SwitchPageHandler(Home recipient, SwitchToOtherPageMessage message)
    {
        SwitchPage(message.Page);
    }

    private void StartupAction(object sender, RoutedEventArgs e)
    {
        homeSelection.IsSelected = true;
    }

    private void SwitchPage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is null)
            return;
        if (args.IsSettingsSelected)
        {
            SwitchPage("setting");
            return;
        }
        string page = (args.SelectedItem as NavigationViewItemBase).Tag.ToString();
        SwitchPage(page);
    }

    public void SwitchPage(string page)
    {
        switch (page)
        {
            case "home":
                contentFrame.Navigate(new MainWindow());
                ViewModel.CurrentPageName = "Home";
                break;
            case "login":
                contentFrame.Navigate(new PleaseLogin());
                ViewModel.CurrentPageName = "Anonymous";
                break;
            case "loggedIn":
                contentFrame.Navigate(new LetMeOut());
                ViewModel.CurrentPageName = SettingManager.Instance.GitUsername;
                break;
            case "setting":
                contentFrame.Navigate(new SettingPage());
                ViewModel.CurrentPageName = "Settings";
                break;
            case "favorite":
                contentFrame.Navigate(new FavoritePage());
                if (ViewModel.GitService.IsAnonymous)
                    ViewModel.CurrentPageName = "Local favorites";
                if (!ViewModel.GitService.IsAnonymous)
                    ViewModel.CurrentPageName = $"{ViewModel.Config.GitUsername}'s favorites";
                break;
            case "upload":
                if (ViewModel.GitService.IsAnonymous)
                {
                    homeSelection.IsSelected = true;
                    MessageBox.Show("I am not letting you in without login to GitHub!",
                        "I don't know how did you manage to do this, but", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    if (contentFrame.Content.GetType().Name != nameof(MainWindow))
                        contentFrame.Navigate(new MainWindow());
                    ViewModel.CurrentPageName = "Home";
                    return;
                }
                contentFrame.Navigate(new UploadPack());
                ViewModel.CurrentPageName = "Upload new pack";
                break;
        }
    }
}
