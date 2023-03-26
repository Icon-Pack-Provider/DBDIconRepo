using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Dialog;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Preview;
using DBDIconRepo.Service;
using DBDIconRepo.ViewModel;
using IconInfo.Icon;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

public partial class RootPages
{
    public RootPages()
    {
        InitializeComponent();
        Messenger.Default.Register<RootPages, SwitchToOtherPageMessage, string>(this, MessageToken.RequestMainPageChange,
            SwitchPageHandler);
        Messenger.Default.Register<RootPages, MonitorForAppFocusMessage, string>(this, MessageToken.RequestSubToAppActivateEvent, SubToAppActivate); 
        Messenger.Default.Register<RootPages, GitUserChangedMessage, string>(this, MessageToken.GitUserChangedToken, UserSwitched);
        this.Activated += ActivationEvent;
        this.Deactivated += DeactivatedEvent;
        
        ViewModel.PropertyChanged += IsBackgroundChangedYet;
        ViewModel.Initialize();
    }

    private void UserSwitched(RootPages recipient, GitUserChangedMessage message)
    {
        if (!OctokitService.Instance.IsAnonymous)
            DialogHelper.Show($"Welcome {SettingManager.Instance.GitUsername}!\r\n" +
                "We are working hard to fix any issues with the app and appreciate your patience.\r\n" +
                "But we're still recommended that you restart the app to ensure optimal experience.", "Login complete!");
        else
            DialogHelper.Show("To ensure optimal performance, we recommend that you restart the app.\r\n" +
                "We are working hard to fix any issues with the app and appreciate your patience.\r\n" +
                "You can restart the app later if you prefer.", "Logout complete!");
        ViewModel.UserInfo = null;
        ViewModel.InitializeUserInfo();
    }

    private void IsBackgroundChangedYet(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Replace navigation pane color to transparent
        if (e.PropertyName == nameof(RootPagesViewModel.BackgroundImage))
        {
            UpdateBackgroundBasedSideNavigationPanel();
            TryUpdateAcrylicTintColor();
        }
    }

    bool madeTransparent = false;
    private void UpdateBackgroundBasedSideNavigationPanel()
    {
        if (string.IsNullOrEmpty(ViewModel.BackgroundImage))
            return; //Still blank?
        if (madeTransparent)
            return;
        if (!madeTransparent)
            madeTransparent = true;
        if (this.Resources.Contains("NavigationViewTopPaneBackground")
            && this.Resources["NavigationViewTopPaneBackground"] is SolidColorBrush navtop)
        {
            navtop.Color = Colors.Transparent;
        }
        if (this.Resources.Contains("NavigationViewDefaultPaneBackground")
            && this.Resources["NavigationViewDefaultPaneBackground"] is SolidColorBrush navbg)
        {
            navbg.Color = Colors.Transparent;
        }
        if (this.Resources.Contains("NavigationViewExpandedPaneBackground")
            && this.Resources["NavigationViewExpandedPaneBackground"] is SolidColorBrush navex)
        {
            navex.Color = Colors.Transparent;
        }
    }

    Action? callOnActivated = null;
    Action? callOnDeactivated = null;
    private void SubToAppActivate(RootPages recipient, MonitorForAppFocusMessage message)
    {
        if (message.Subscribe)
        {
            callOnActivated = message.CallOnActivate;
            callOnDeactivated = message.CallOnDeactivated;
        }
        else
        {
            callOnActivated = null;
            callOnDeactivated= null;
        }
    }

    private void SwitchPageHandler(RootPages recipient, SwitchToOtherPageMessage message)
    {
        if (message.Page == "home")
            homeSelection.IsSelected = true;
        SwitchPage(message.Page);
    }

    private void StartupAction(object sender, RoutedEventArgs e)
    {
        //Load stuffs
        var packTask = IconPack.Packs.GetPacks((notif) =>
        {
            ViewModel.ProgressText += $"{DateTime.Now:G}: [Packs] {notif}\r\n";
        });
        var addonTask = SelectionListing.Lists.CheckCatagoryRepo((notif) =>
        {
            ViewModel.ProgressText += $"{DateTime.Now:G}: [Addon] {notif}\r\n";
        });

        Task.WhenAll(packTask, addonTask).Await(() =>
        {
            Logger.Write(ViewModel.ProgressText);
            Application.Current.Dispatcher.Invoke(() =>
            {
                homeSelection.IsSelected = true;
                ViewModel.ProgressText = string.Empty;
                ViewModel.IsInitializing = true;
            });
            //Check for new background maybe?
            ViewModel.BackgroundImage = BackgroundRandomizer.Get(forceRecheck: true);
            UpdateBackgroundBasedSideNavigationPanel();
        },
        (error) =>
        {
            Logger.Write(ViewModel.ProgressText);
            Logger.Write($"{error.Message}\r\n{error.StackTrace}");
            ViewModel.ProgressText += "\r\n" + error.Message;
            ViewModel.IsInitializing = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                homeSelection.IsSelected = true;
                if (error.Message.Contains("API rate limit exceeded"))
                {
                    this.Title = $"Dead by daylight: Icon repo | GitHub rate limit exceeded. Many function might not work properly!";
                }
            });
        });
        //Background check
        if (string.IsNullOrEmpty(ViewModel.BackgroundImage))
        {
            PaneBackgroundImitator.SetResourceReference(Rectangle.FillProperty, "SystemControlPageBackgroundChromeMediumLowBrush");
        }
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
                contentFrame.Navigate(new Home());
                ViewModel.CurrentPageName = "Home";
                break;
            //case "login":
            //    contentFrame.Navigate(new PleaseLogin());
            //    ViewModel.CurrentPageName = "Anonymous";
            //    break;
            //case "loggedIn":
            //    contentFrame.Navigate(new LetMeOut());
            //    ViewModel.CurrentPageName = SettingManager.Instance.GitUsername;
            //    break;
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
            case "history":
                contentFrame.Navigate(new History());
                ViewModel.CurrentPageName = "History";
                break;
            case "update":
                if (AnonymousWarned())
                    break;
                contentFrame.Navigate(new UpdatePack());
                ViewModel.CurrentPageName = "Update existing pack";
                break;
            case "upload":
                if (AnonymousWarned())
                    break;
                contentFrame.Navigate(new UploadPack());
                ViewModel.CurrentPageName = "Upload new pack";
                break;
        }
    }

    private bool AnonymousWarned()
    {
        if (ViewModel.GitService.IsAnonymous)
        {
            homeSelection.IsSelected = true;
            DialogHelper.Show("I am not letting you in without login to GitHub!",
                "I don't know how did you manage to do this, but",
                DialogSymbol.Error);
            if (contentFrame.Content.GetType().Name != nameof(Home))
                contentFrame.Navigate(new Home());
            ViewModel.CurrentPageName = "Home";
            return true;
        }
        return false;
    }

    private void DeactivatedEvent(object? sender, System.EventArgs e)
    {
        callOnDeactivated?.Invoke();
    }

    private void ActivationEvent(object? sender, System.EventArgs e)
    {
        callOnActivated?.Invoke();
        ViewModel.CheckIfDBDRunning();
    }

    private void animatePaneExpanding(NavigationView sender, object args)
    {
        var board = PaneBackgroundImitator.TryFindResource("animatePaneExpand") as Storyboard;
        board.Begin();
    }

    private void animatePaneShrinking(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        var board = PaneBackgroundImitator.TryFindResource("animatePaneShrink") as Storyboard;
        board.Begin();
    }

    private void OpenNewVersionLink(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.OpenAppReleasePage();
    }

    private void TryUpdateAcrylicColor(object sender, RoutedEventArgs e)
    {
        TryUpdateAcrylicTintColor();
    }

    private void TryUpdateAcrylicTintColor()
    {
        if (ViewModel is null)
            return;
        if (string.IsNullOrEmpty(ViewModel.BackgroundImage)) //No need to do its if background is blank
            return;
        if (this.Resources["DefinedTintOpacity"] is double d)
        {
            //Set opacity, if Light:0.75; otherwise Dark:0
            var currentTheme = ModernWpf.ThemeManager.GetActualTheme(this);
            ((PaneBackgroundImitator.Fill as VisualBrush)
                .Visual as SourceChord.FluentWPF.AcrylicPanel)
                .TintOpacity = currentTheme == ElementTheme.Dark ? 0d : 0.75d;
        }
    }

    private void mainPane_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer == loggedInNavItem)
        {
            FlyoutBase.ShowAttachedFlyout(loggedInNavItem);
        }
        else if (args.InvokedItemContainer is NavigationViewItemBase nav)
        {
            if (nav.Tag is not string tagInfo)
                return;

            if (tagInfo.StartsWith("login") && ViewModel.UserInfo is AnonymousUserViewModel anon)
            {
                if (tagInfo == "login_oauth")
                    anon.LoginToGithubCommand.Execute(null);
                else if (tagInfo == "login_token")
                    anon.ManuallyLoginToGithubCommand.Execute(null);
            }
            else if (tagInfo == "logout_user")
            {
                if (ViewModel.UserInfo is not UserViewModel userVM)
                    return;
                userVM.LogoutOfGithubCommand.Execute(null);
            }
        }
    }

}
