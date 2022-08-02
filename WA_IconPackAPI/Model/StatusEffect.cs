using CommunityToolkit.Mvvm.ComponentModel;
using IconPackAPI.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPackAPI.Model
{
    public partial class StatusEffect : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
