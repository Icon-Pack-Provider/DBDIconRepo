using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.ViewModel;
using System;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for GettingStarted.xaml
/// </summary>
[INotifyPropertyChanged]
public partial class GettingStarted : Window
{
    public GettingStarted()
    {
        InitializeComponent();
        DataContext = this;
        Messenger.Default.Register<GettingStarted, GitUserChangedMessage, string>(this, MessageToken.GitUserChangedToken, UserChangedHandler);
    }

    private void UserChangedHandler(GettingStarted recipient, GitUserChangedMessage message)
    {
        RootPages rp = new();
        rp.Show();
        this.Close();
    }

    public AnonymousUserViewModel? AnonViewModel { get; set; } = new();

    [ObservableProperty] private bool showFAQ;

    partial void OnShowFAQChanged(bool value)
    {
        ShowAnonContinue = true;
    }

    [ObservableProperty] private bool showAnonContinue;

    [RelayCommand]
    private void ContinueAsAnonymous()
    {
        SettingManager.Instance.LandedOnLandingPageBefore = true;
        SettingManager.SaveSettings();
        RootPages rp = new();
        rp.Show();
        this.Close();
    }

    [RelayCommand]
    private void Login()
    {
        AnonViewModel.LoginToGithubCommand?.Execute(null);
        SettingManager.Instance.LandedOnLandingPageBefore = true;
        SettingManager.SaveSettings();
    }

    [RelayCommand]
    private void OpenPacksURL()
    {
        URL.OpenURL("https://github.com/topics/dbd-icon-pack");
    }
}
