using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.ViewModel;

public partial class HistoryViewModel : HomeViewModel
{
    public HistoryViewModel() 
    {
        Initialize();
    }

    [ObservableProperty]
    ObservableCollection<IHistoryItem>? histories = new();

    private bool _isInitialize = false;
    public void Initialize()
    {
        if (_isInitialize) { return; }
        _isInitialize = true;

        //Load history
        Histories = HistoryLogger.LoadHistory();
    }
}
