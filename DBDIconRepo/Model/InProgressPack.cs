using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Model
{
    public class InProgressPack : IconPack.Model.Pack
    {
        [ObservableProperty]
        string workingDirectory = string.Empty;


    }
}
