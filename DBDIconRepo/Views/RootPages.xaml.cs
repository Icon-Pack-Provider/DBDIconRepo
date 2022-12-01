using DBDIconRepo.Dialog;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Preview;
using IconInfo.Icon;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

public partial class RootPages : Window
{
    private static bool attempt = false;
    public RootPages()
    {
        InitializeComponent();
        Messenger.Default.Register<RootPages, SwitchToOtherPageMessage, string>(this, MessageToken.RequestMainPageChange,
            SwitchPageHandler);
        Messenger.Default.Register<RootPages, MonitorForAppFocusMessage, string>(this, MessageToken.RequestSubToAppActivateEvent, SubToAppActivate);
        this.Activated += ActivationEvent;
        this.Deactivated += DeactivatedEvent;
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
                    DialogHelper.Show("I am not letting you in without login to GitHub!", 
                        "I don't know how did you manage to do this, but", 
                        DialogSymbol.Error);
                    if (contentFrame.Content.GetType().Name != nameof(Home))
                        contentFrame.Navigate(new Home());
                    ViewModel.CurrentPageName = "Home";
                    return;
                }
                contentFrame.Navigate(new UploadPack());
                ViewModel.CurrentPageName = "Upload new pack";
                break;
        }
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

}
