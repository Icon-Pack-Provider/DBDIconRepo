using CommunityToolkit.Mvvm.Messaging;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
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
    }

    private bool _isInitialized = false;
    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
                
    }
}
