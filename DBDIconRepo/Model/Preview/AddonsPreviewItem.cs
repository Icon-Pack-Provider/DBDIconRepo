using IconPack.Model;

namespace DBDIconRepo.Model.Preview;

public partial class AddonPreviewItem : BasePreview
{
    public AddonPreviewItem(string path, PackRepositoryInfo repo) : base(path, repo)
    {
    }
}
