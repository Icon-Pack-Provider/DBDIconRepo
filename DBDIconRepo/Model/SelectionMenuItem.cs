using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DBDIconRepo.Model
{
    public class SelectionMenuItem : ObservableObject
    {
        string? _name;
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        char? _icon;
        public char? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        List<string>? _selectionRecommends;
        public List<string>? SelectionRecommended
        {
            get => _selectionRecommends;
            private set => _selectionRecommends = value;
        }

        ObservableCollection<SelectionMenuItem>? _childs;
        public ObservableCollection<SelectionMenuItem>? Childs
        {
            get => _childs;
            set => SetProperty(ref _childs, value);
        }

        //All null is for separator
        public SelectionMenuItem()
        {
            Name = null;
            Icon = null;
            SelectionRecommended = null;
            Childs = null;
        }
    }
}
