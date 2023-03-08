namespace DBDIconRepo.Model.Uploadable;

public interface IUploadableItem
{
    /// <summary>
    /// Use as folder path eg. Perks, CharPortraits
    /// </summary>
    string Name { get; set; }
    /// <summary>
    /// Explain what it was eg.Perks icon, Portrait icons etc.
    /// </summary>
    string DisplayName { get; set; }
    bool? IsSelected { get; set; }
    bool IsExpand { get; set; }
    UploadableFolder? Parent { get; }
}