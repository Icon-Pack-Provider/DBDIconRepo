using IconPack.Model;
using System.Collections.Generic;

namespace DBDIconRepo.Model
{
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
}
