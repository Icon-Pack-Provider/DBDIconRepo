﻿using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Internal;

namespace IconPack.Model
{
    public partial class Power : ObservableObject, IBasic, IFolder
    {
#nullable enable
        [ObservableProperty]
        string? folder;
#nullable disable

        [ObservableProperty]
        string file;

        [ObservableProperty] //Power name
        string name;

        [ObservableProperty] //Killer name that use this power
        string owner;
    }
}
