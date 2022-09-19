using IconPack.Model;

namespace DBDIconRepo.Model.Preview;

public class EmblemPreviewItem : BasePreview
{
    public EmblemPreviewItem(string path, PackRepositoryInfo repo) : base(path, repo)
    {
    }
}

public enum EmblemType
{
    None,
    Silver,
    Bronze,
    Gold,
    Iridescent
}
