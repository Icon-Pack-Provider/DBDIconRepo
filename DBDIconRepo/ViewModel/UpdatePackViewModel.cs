using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DBDIconRepo.ViewModel;

public partial class UpdatePackViewModel : ObservableObject
{
    public OctokitService Git => Singleton<OctokitService>.Instance;
    public Setting Config => SettingManager.Instance;

    #region Pages
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnSetNewIconsDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnInvalidIconDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnSelectNewIcons))]
    [NotifyPropertyChangedFor(nameof(ShowOnCommitMessage))]
    private UpdatePages currentPage = UpdatePages.SetNewIconsDirectory;

    public Visibility ShowOnSetNewIconsDirectory => CurrentPage == UpdatePages.SetNewIconsDirectory ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnInvalidIconDirectory => CurrentPage == UpdatePages.InvalidIconDirectory ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnSelectNewIcons => CurrentPage == UpdatePages.SelectNewIcons ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnCommitMessage => CurrentPage == UpdatePages.CommitMessage ? Visibility.Visible : Visibility.Collapsed;


    #endregion
}

public enum UpdatePages
{
    SetNewIconsDirectory,
    InvalidIconDirectory,
    SelectNewIcons,
    CommitMessage
}