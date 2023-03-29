namespace DBDIconRepo.Model;

public class MessageToken
{
    public const string FILTEROPTIONSCHANGETOKEN = "FILTEROPTIONCHANGED";
    public const string SETTINGVALUECHANGETOKEN = "SETTINGVALUECHANGED";
    public const string REQUESTSEARCHQUERYTOKEN = "REQUESTEDSEARCHQUERY";
    public const string REQUESTDOWNLOADREPOTOKEN = "REQUESTEDDOWNLOADREPO";
    public const string REPOSITORYDOWNLOADREPORTTOKEN = "REPOSITORYDOWNLOADING";
    public const string REQUESTVIEWPACKDETAIL = "OPENNINGPACKDETAILWINDOWNOW";
    public const string REPOWAITFORINSTALLTOKEN = "REPOWAITINGFORINSTALL";
    public const string REPORTINSTALLPACKTOKEN = "REPORTINSTALLINGPACK";

    public const string RequestMainPageChange = "PLEASEGOTOOTHERPAGE";

    public const string MassRepoStarChangedToken = "MassRepoStarChanged";
    public const string RepoStarChangedToken = "RepoStarChanged";

    public const string RequestSubToAppActivateEvent = "PleaseMonitorAppActivateDeactivate";
    public const string RequestUnSubToAppActivateEvent = "PleaseStopMonitorAppActivateDeactivate";
    public const string SendDialogResponseToken = "ResponseWithThis";

    public const string GitUserChangedToken = "Yo!NewUserWhosDis?";
    public const string AttemptReloadIconMessage = "Hol'UpASecAndReloadThisAgain";
}
