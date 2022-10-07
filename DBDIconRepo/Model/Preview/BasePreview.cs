using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconInfo.Internal;
using IconPack.Model;
using System;
using System.Threading.Tasks;

namespace DBDIconRepo.Model.Preview;

public partial class BasePreview : ObservableObject, IBasePreview
{
    [ObservableProperty]
    IBasic info;

    [ObservableProperty]
    string? iconURL;

    public int DecodeWidth => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Width);
    public int DecodeHeight => Convert.ToInt32(IconResolutionScale.GetResolutionScale(type).Height);

    private string type = "perk";

    public BasePreview(string path, PackRepositoryInfo repo)
    {
        IconURL = URL.GetIconAsGitRawContent(repo, path);
        if (path.ToLower().Contains(".banner"))
            type = "banner";
        else
        {
            var checker = IconTypeIdentify.FromPath(path);
            type = checker.GetType().Name.ToLower();
        }
    }

    private bool _loadingImage;
    private byte[] _loadedImage;
    public byte[] LoadedImage
    {
        get
        {
            if (_loadedImage != null || _loadingImage)
            {
                return _loadedImage;
            }
            Action<Task<byte[]>> loading = async task =>
            {
                _loadedImage = await task;
                OnPropertyChanged(nameof(LoadedImage));
            };

            URL.LoadImageAsBytesFromOnline(IconURL)
                .ContinueWith(loading,
                TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(_ => _loadingImage = false)
                .ConfigureAwait(false);
            return null;
        }
    }
}

public interface IBasePreview
{
    IBasic Info { get; set; }
    string? IconURL { get; set; }
    int DecodeWidth { get; }
    int DecodeHeight { get; }
    byte[] LoadedImage { get; }
}