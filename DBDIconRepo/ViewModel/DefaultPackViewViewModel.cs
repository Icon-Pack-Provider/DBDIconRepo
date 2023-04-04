using CommunityToolkit.Mvvm.Messaging;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.ViewModel;

public class DefaultPackViewViewModel : HomeViewModel
{
    public DefaultPackViewViewModel()
    {
        Initialize();
    }

    public DefaultPackViewViewModel(Task<Pack[]> packGatherMethod, PackDisplayComponentOptions compOption)
        : base(packGatherMethod, compOption)
    {
        Initialize();
        Task.Run(async () =>
        {
            var packs = await packGatherMethod;

            AllAvailablePack = new();
            foreach (var pack in packs)
            {
                PackDisplay pd = new(pack, compOption);
                if (pd is null) //Somehow??
                    continue;
                //Check readme
                //Banner and urls
                await pd.GatherPreview();
                AllAvailablePack.Add(pd);
            }
        }).Await(() =>
        {
            GettingPacks = false;
            OnPropertyChanged(nameof(FilteredList));
        }, (e) =>
        {
            GettingPacks = false;
            OnPropertyChanged(nameof(FilteredList));
        });
    }

    private bool _isInitialized = false;
    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
                
    }

    public new ObservableCollection<PackDisplay> FilteredList
    {
        get
        {
            if (AllAvailablePack is null)
                return new ObservableCollection<PackDisplay>();

            List<PackDisplay> filtering = new(AllAvailablePack);
            return new ObservableCollection<PackDisplay>(filtering);
        }
    }
}
