using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel;

public partial class FavoriteViewModel : HomeViewModel
{
    public FavoriteViewModel()
    {
        Initialize();
    }

    public FavoriteViewModel(Task<Pack[]> packGatherMethod, PackDisplayComponentOptions compOption)
        : base(packGatherMethod, compOption)
    {
        Initialize();
    }

    private bool _isInitialized = false;
    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
        Messenger.Default.Register<FavoriteViewModel, RepoStarChangedMessage, string>(this, MessageToken.RepoStarChangedToken, HandleStarUpdate);
    }

    private void HandleStarUpdate(FavoriteViewModel recipient, RepoStarChangedMessage message)
    {
        if (message.IsStarred)
        {
            _favs = null;
            OnPropertyChanged(nameof(FilteredList));
        }
        else
        {
            var inOrigin = _favs.FirstOrDefault(pack => pack.Info.Repository.ID == message.Changed.ID);
            if (inOrigin is not null)
            {
                //Remove it
                _favs.Remove(inOrigin);
                OnPropertyChanged(nameof(FilteredList));
            }
        }
    }

    private ObservableCollection<PackDisplay>? _favs = null;
    public new ObservableCollection<PackDisplay> FilteredList
    {
        get
        {
            if (AllAvailablePack is null)
                return new ObservableCollection<PackDisplay>();
            if (_favs is null || _favs.Count < 1)
            {
                _favs = new(AllAvailablePack
                        .Where(pack => Star.BaseService.AllStarred.Any(i => i.ID == pack.Info.Repository.ID))
                        .ToList());
            }
            IsFilteredListEmpty = _favs.Count == 0;
            return _favs;
        }
    }

    public StarService Star => StarService.Instance;
    public OctokitService Git => OctokitService.Instance;

    [RelayCommand]
    private void IDGAF()
    {
        Config.DismissedTheFavoritePageHeaderPrompt = true;
    }
}
