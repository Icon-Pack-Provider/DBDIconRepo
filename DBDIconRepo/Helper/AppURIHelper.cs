using System;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public static class AppURIHelper
{
    private const string AppScheme = "dbdiconrepo";
    public static UriRequest Read(string uri)
    {
        string unescape = Uri.UnescapeDataString(uri);
        Uri info = new(unescape);
        if (info.Scheme != AppScheme)
            return new();
        switch (info.Host)
        {
            case "authenticate":
                var splices = info.Query.Split(new char[] { '=', '?', '&' }, StringSplitOptions.RemoveEmptyEntries);
                return new AuthRequest()
                {
                    Code = splices[1],
                    State = splices[3]
                };
            case "navigation":
            case "home":
                //TODO:Handle other page navigation
                return new NavigationRequest(info);
            case "restore":
                return new RestoreWindowRequest();
            case "restart":
                return new RestartAppRequest();
            case "note":
                return new ReadTheNoteRequest();
            default:
                return new();
        }
    }
}

public class UriRequest
{
    public RequestType Type { get; set; }

    public UriRequest()
    {
        Type = RequestType.None;
    }
}

public class NavigationRequest : UriRequest
{
    public string Page { get; set; } = "home";
    public NavigationRequest()
    {
        Type = RequestType.Navigation;        
    }

    public NavigationRequest(Uri extracted)
    {
        Type = RequestType.Navigation;
        if (extracted.Segments.Length == 1)
        {
            return;
        }
        else if (extracted.Segments.Length >= 2)
        {
            switch (extracted.Segments[1])
            {
                //Literal name
                case "upload":
                case "update":
                case "setting":
                case "home":
                case "history":
                case "favorite":
                    Page = extracted.Segments[1];
                    return;
                //Alternate names
                case "main":
                case "index":
                    Page = "home";
                    return;
                case "favourite":
                case "fav":
                    Page = "favorite";
                    return;
                case "config":
                case "preference":
                case "pref":
                case "cfg":
                case "set":
                    Page = "setting";
                    return;
            }
        }
    }
}

public class AuthRequest : UriRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public AuthRequest()
    {
        Type = RequestType.Authentication;
    }
}

public class RestoreWindowRequest : UriRequest
{
    public RestoreWindowRequest()
    {
        Type = RequestType.Restore;
    }
}

public class RestartAppRequest : UriRequest
{
    public RestartAppRequest() { Type = RequestType.Restart; }
}

public class ReadTheNoteRequest : UriRequest
{
    public ReadTheNoteRequest() { Type = RequestType.ReadNote; }
}

public enum RequestType
{
    None,
    Navigation,
    Query,
    Authentication,
    Restore,
    Restart,
    ReadNote
}