using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Model;
using System;
using System.Collections.ObjectModel;

namespace DBDIconRepo.Model.History;
public partial class HistoryViewPack : ObservableObject, IHistoryItem
{
    [ObservableProperty]
    private long victim;

    [ObservableProperty]
    private HistoryType action;

    [ObservableProperty]
    private DateTime time;

    public HistoryViewPack(Pack? pack)
    {
        Victim = pack.Repository.ID;
        Action = HistoryType.ViewDetail;
        Time = DateTime.Now;
    }

    public HistoryViewPack() { }
}