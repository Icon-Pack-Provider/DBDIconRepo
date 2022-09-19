using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconPack.Model;
using System.Linq;

namespace DBDIconRepo.Model
{
    public interface IPackSelectionItem
    {
        string? FullPath { get; set; }
        string? FilePath { get; }
        string? Name { get; set; }
        bool? IsSelected { get; set; }
        IBaseItemInfo? Info { get; set; }
    }

    public class PackSelectionFile : ObservableObject, IPackSelectionItem
    {
        string? _fullPath;
        /// <summary>
        /// Filename
        /// </summary>
        public string? FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        public string FilePath => FullPath.Replace('/', '\\');

        string? _name;
        /// <summary>
        /// Filename
        /// </summary>
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        bool? _selected;
        public bool? IsSelected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        IBaseItemInfo? _info;
        public IBaseItemInfo? Info
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
            Name = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries).Last().Replace(".png", "");
            IsSelected = true;
            Info = PackSelectionHelper.GetItemInfo(path);
        }
    }
}
