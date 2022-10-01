using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionListing.Model
{
    public partial class SelectionMenuItem : ObservableObject
    {
#nullable disable
        [ObservableProperty]
        private string name;
#nullable enable
        [ObservableProperty]
        private string? displayName;

        [ObservableProperty]
        private ObservableCollection<SelectionMenuItem?> childs = null;

        [ObservableProperty]
        private ObservableCollection<string> selections;
    }
}
