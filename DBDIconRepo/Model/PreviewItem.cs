using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconPack.Model;
using System;
using System.Linq;
using Info = IconPack.Helper.Info;

namespace DBDIconRepo.Model
{
    public partial class PreviewItem : ObservableObject
    {
        IBasic? _info;
        public IBasic Info
        {
            get => _info;
            set
            {
                if (SetProperty(ref _info, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Name
        {
            get
            {
                if (Info is null)
                    return string.Empty;
                return Info.Name;
            }
        }
    }
}
