using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Service;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Model;

public partial class OnlineSourceDisplay : ObservableObject, IDisplayItem
{
    public OnlineSourceDisplay() { }

    public OnlineSourceDisplay(string url)
    {
        PropertyChanged += UpdateOnURL;
        URL = url;
        Messenger.Default.Register<OnlineSourceDisplay, AttemptReloadIconMessage, string>(this, MessageToken.AttemptReloadIconMessage, TryReloadAgainInAFew);
    }

    private async void TryReloadAgainInAFew(object recipient, AttemptReloadIconMessage message)
    {
        if (message.URL != URL)
            return;
        string path = string.Empty;
        Task.Run(async () =>
        {
            path = await ImageCacheHelper.GetImage(URL);
        }).Await(() =>
        {
            if (path == "TIMEOUT")
                Messenger.Default.Send(new AttemptReloadIconMessage(URL), MessageToken.AttemptReloadIconMessage);
            else
                LocalizedURL = path;
        },
        (e) =>
        {
            Messenger.Default.Send(new AttemptReloadIconMessage(URL), MessageToken.AttemptReloadIconMessage);
        });
    }

    private async void UpdateOnURL(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(URL))
            return;
        if (string.IsNullOrEmpty(URL))
            return;
        if (OctokitService.Instance.IsAnonymous)
        {
            LocalizedURL = "TIMEOUT";
            return;
        }
        if (URL.StartsWith("http"))
        {
            LocalizedURL = await ImageCacheHelper.GetImage(URL);
        }
    }

    [ObservableProperty]
    string? _uRL = string.Empty;
    [ObservableProperty]
    string? localizedURL = string.Empty;
}
