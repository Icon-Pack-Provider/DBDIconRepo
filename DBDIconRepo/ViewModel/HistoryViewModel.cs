using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model.History;
using DBDIconRepo.Service;
using IconPack;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;

namespace DBDIconRepo.ViewModel;

public partial class HistoryViewModel : HomeViewModel
{
    public HistoryViewModel() 
    {
        Initialize();
    }

    [ObservableProperty]
    ReadOnlyObservableCollection<Pack?> availablePacks;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoHistoryMadeYet))]
    ObservableCollection<IHistoryItem>? histories = new();

    public Visibility NoHistoryMadeYet => Histories.Count < 1 ? Visibility.Visible : Visibility.Collapsed;

    private bool _isInitialize = false;
    public void Initialize()
    {
        if (_isInitialize) { return; }
        _isInitialize = true;

        Task.Run(async () =>
        {
            AvailablePacks = new(await Packs.GetPacks());
        }).Await(async () =>
        {
            //Load history
            var histories = HistoryLogger.LoadHistory().OrderBy(i => i.Time);
            Histories = new(histories);
            IsGettingPacks = Visibility.Collapsed;
        });
    }

    [ObservableProperty]
    private Visibility isGettingPacks = Visibility.Visible;
}
