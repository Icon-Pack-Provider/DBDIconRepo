using DBDIconRepo.Dialog;
using IconPack.Model;
using IconPack.Model.Progress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.Model;

public class FilterOptionChangedMessage
{
    public string KeyChanged { get; private set; }
    public FilterOptions Changed { get; private set; }

    public FilterOptionChangedMessage(string key, FilterOptions value)
    {
        KeyChanged = key;
        Changed = value;
    }
}

public class SettingChangedMessage
{
    public string? PropertyName { get; private set; }
    public object? Value { get; private set; }

    public SettingChangedMessage(string? property, object? value)
    {
        PropertyName = property;
        Value = value;
    }
}

public class RequestSearchQueryMessage
{
    public string? Query { get; private set; }

    public RequestSearchQueryMessage(string? _required)
    {
        Query = _required;
    }
}

public class RequestDownloadRepo
{
    public Pack Info { get; private set; }

    public RequestDownloadRepo(Pack _pack)
    {
        Info = _pack;
    }
}


public class DownloadRepoProgressReportMessage
{
    public DownloadState CurrentState { get; private set; }
    public double EstimateProgress { get; private set; }

    public DownloadRepoProgressReportMessage(ICloningProgress progress)
    {
        switch (progress)
        {
            case ACounting a: 
                CurrentState = DownloadState.Enumerating; 
                break;
            case BCompressing b:
                CurrentState = DownloadState.Compressing;
                break;
            case CTransfer c: 
                CurrentState = DownloadState.Transfering; 
                break;
            case DCheckingOut d:
                CurrentState = DownloadState.CheckingOut; 
                break;
            default:
                CurrentState = DownloadState.Done;
                break;
        }
        EstimateProgress = progress.Percent;
    }

    public DownloadRepoProgressReportMessage(DownloadState currentState, double estimateProgress)
    {
        CurrentState = currentState;
        EstimateProgress = estimateProgress;
    }
    public DownloadRepoProgressReportMessage()
    {
        CurrentState = DownloadState.Transfering;
        EstimateProgress = -1;
    }
}

public class IndetermineRepoProgressReportMessage : DownloadRepoProgressReportMessage
{
    public IndetermineRepoProgressReportMessage()
    {
    }
}

public class WaitForInstallMessage
{
    public IList<IPackSelectionItem> PackInstallSelection { get; private set; }

    public WaitForInstallMessage(IList<IPackSelectionItem> selections)
        => PackInstallSelection = selections;
}

public class RequestViewPackDetailMessage
{
    public Pack? Selected { get; private set; }
    public RequestViewPackDetailMessage(Pack? requested)
    {
        Selected = requested;
    }
}

public class InstallationProgressReportMessage
{
    public string Filename { get; private set; }
    public int TotalInstall { get; private set; }

    public InstallationProgressReportMessage(string name, int total)
    {
        Filename = name;
        TotalInstall = total;
    }
}

public enum DownloadState
{
    Enumerating,
    Compressing,
    Transfering,
    CheckingOut,
    Done
}

public class SwitchToOtherPageMessage
{
    public string Page { get; private set; }
    public SwitchToOtherPageMessage(string page)
    {
        Page = page;
    }
}

public class MassRepoStarChanged
{
    public PackRepositoryInfo[] Starred { get; private set; }
    public PackRepositoryInfo[] UnStarred { get; private set; }
    public MassRepoStarChanged(List<PackRepositoryInfo> preChanged, List<PackRepositoryInfo> postChanged)
    {
        var same = preChanged.Intersect(postChanged);
        preChanged.RemoveAll(i => same.Contains(i));
        postChanged.RemoveAll(i => same.Contains(i));
        Starred = postChanged.ToArray();
        UnStarred = preChanged.ToArray();
    }
}

public class RepoStarChangedMessage
{
    public PackRepositoryInfo Changed { get; private set; }
    public bool IsStarred { get; private set; }

    public RepoStarChangedMessage(PackRepositoryInfo changed, bool isStarred)
    {
        Changed = changed;
        IsStarred = isStarred;
    }
}

public class MonitorForAppFocusMessage
{
    public bool Subscribe { get; private set; }
    public Action? CallOnActivate { get; private set; }
    public Action? CallOnDeactivated { get; private set; }

    public MonitorForAppFocusMessage(Action? callOnActivate = null, Action? callOnDeactivated = null)
    {
        Subscribe = true;
        CallOnActivate = callOnActivate;
        CallOnDeactivated = callOnDeactivated;
    }

    public MonitorForAppFocusMessage(bool sub)
        => Subscribe = sub;
}

public class DialogResponseMessage
{
    public DialogResponse Response { get; }
    public DialogResponseMessage(DialogResponse response)
    {
        Response = response;
    }
}