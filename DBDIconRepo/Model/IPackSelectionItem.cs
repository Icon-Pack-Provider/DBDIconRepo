using IconPack.Model;

namespace DBDIconRepo.Model
{
    public interface IPackSelectionItem
    {
        string? FullPath { get; set; }
        string? FilePath { get; }
        string? Name { get; set; }
        bool? IsSelected { get; set; }
        IBasic? Info { get; set; }
    }
}
