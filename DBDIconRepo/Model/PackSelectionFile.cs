using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconPack.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Model
{
    public class PackSelectionFile : ObservableObject, IPackSelectionItem
    {
        string? _fullPath;
        public string? FullPath
        {
            get => _fullPath;
            set
            {
                if (SetProperty(ref _fullPath, value))
                {
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string FilePath => 
            FullPath is null ? string.Empty : FullPath.Replace('/', '\\');

        /// <summary>
        /// Filename
        /// </summary>
        string? _name;
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        bool? _isSelected;
        public bool? IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        IBasic? _info;
        public IBasic? Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        public PackSelectionFile()
        {
            IsSelected = true;
        }

        public PackSelectionFile(string path) 
        {
            FullPath = path;
            Name = PackSelectionHelper.GetPathWithoutExtension(path);
            IsSelected = true;
            Info = PackSelectionHelper.GetItemInfo(path);
        }
    }
}
