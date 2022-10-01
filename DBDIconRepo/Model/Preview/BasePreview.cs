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

    public int DecodeWidth => SettingManager.Instance.IconPreviewDecodeWidth;
    public int DecodeHeight => SettingManager.Instance.IconPreviewDecodeHeight;

    public BasePreview(string path, PackRepositoryInfo repo)
    {
        IconURL = URL.GetIconAsGitRawContent(repo, path);
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

            CacheOrGit.LoadImage(IconURL)
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