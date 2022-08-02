﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace IconPack.Model
{
    public partial class StatusEffect : ObservableObject, IBasic
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string file;
    }
}
