using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DBDIconRepo.ViewModel;

public partial class HistoryViewModel : HomeViewModel
{
    public HistoryViewModel() 
    {
        Initialize();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoHistoryMadeYet))]
    ObservableCollection<IHistoryItem>? histories = new();

    public Visibility NoHistoryMadeYet => Histories.Count < 1 ? Visibility.Visible : Visibility.Collapsed;

    private bool _isInitialize = false;
    public void Initialize()
    {
        if (_isInitialize) { return; }
        _isInitialize = true;

        //Load history
        Histories = HistoryLogger.LoadHistory();
        IsGettingPacks = Visibility.Collapsed;
        OnPropertyChanged(nameof(NoHistoryMadeYet));
    }

    [ObservableProperty]
    private Visibility isGettingPacks = Visibility.Visible;
}
