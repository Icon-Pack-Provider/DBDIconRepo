using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;
using IconPack.Internal;
using IconPack.Internal.Helper;
using Octokit;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace IconPack.Model;

public partial class Pack : ObservableObject
{
    string? name;
    public string? Name
    {
        get
        {
            if (Overrides is not null)
                return Overrides.Name;
            return name;
        }
        set => SetProperty(ref name, value);
    }

    string? description;
    public string? Description
    {
        get
        {
            if (Overrides is not null)
                return Overrides.Description;
            return description;
        }
        set
        {
            SetProperty(ref description, value);
        }
    }

    string? author;
    public string? Author
    {
        get
        {
            if (author is null && Repository is not null)
                return Repository.Owner;
            return author;
        }
        set => SetProperty(ref author, value);
    }

    [ObservableProperty]
    string? uRL;

    DateTime _lastUpdate;
    [JsonConverter(typeof(CustomDateTimeFormat))]
    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set => SetProperty(ref _lastUpdate, value);
    }

    [ObservableProperty]
    PackRepositoryInfo? repository;

    [ObservableProperty]
    PackContentInfo? contentInfo;

    [ObservableProperty]
    OverrideProperties? overrides = null;
}

public partial class OverrideProperties : ObservableObject
{
    [ObservableProperty]
    string? name;

    [ObservableProperty]
    string? description;

    [ObservableProperty]
    AuthorDisplayType authorMode = AuthorDisplayType.OnlyOwner;

    [ObservableProperty]
    ObservableCollection<string>? authors = new();

    [ObservableProperty]
    ObservableCollection<string>? displayFiles = new();
}

public enum AuthorDisplayType
{
    OnlyOwner,
    OwnerAndTop3Contributor,
    OwnerAndAllContributor
}

public partial class PackRepositoryInfo : ObservableObject
{
    public PackRepositoryInfo() { }
    public PackRepositoryInfo(Repository repo)
    { //Find out how it keep getting main instead of master
        
        ID = repo.Id;
        Name = repo.Name;
        Owner = repo.Owner.Login;
        DefaultBranch = repo.DefaultBranch;
        CloneUrl = repo.CloneUrl;
    }

    [ObservableProperty]
    string? name;

    [ObservableProperty]
    long iD;

    [ObservableProperty]
    string? owner;

    [ObservableProperty]
    string? defaultBranch;

    [ObservableProperty]
    string? cloneUrl;
}

public partial class PackContentInfo : ObservableObject
{
    public PackContentInfo() { }

    public static async Task<PackContentInfo> GetContentInfo(Repository repo)
    {
        ThrowHelper.APINotInitialize();

        var commits = await OctokitService.Instance.Client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
        var head = commits.First();
        var treeContent = await OctokitService.Instance.Client.Git.Tree.GetRecursive(repo.Id, head.Sha);
        var treeOnly = treeContent.Tree.Where(i => i.Type.Value == TreeType.Tree);

        return new PackContentInfo()
        {
            HasAddons = treeOnly.Any(content => content.Path == "ItemAddons"),
            HasItems = treeOnly.Any(content => content.Path == "items"),
            HasOfferings = treeOnly.Any(content => content.Path == "Favors"),
            HasPerks = treeOnly.Any(content => content.Path == "Perks"),
            HasPortraits = treeOnly.Any(content => content.Path == "CharPortraits"),
            HasPowers = treeOnly.Any(content => content.Path == "Powers"),
            HasStatus = treeOnly.Any(content => content.Path == "StatusEffects"),
            Files = new ObservableCollection<string>(treeContent.Tree.Where(i => i.Type.Value == TreeType.Blob).Where(i => i.Path.EndsWith(".png")).Select(i => i.Path))
        };
    }

    [ObservableProperty]
    bool hasPerks;

    [ObservableProperty]
    bool hasPortraits;

    [ObservableProperty]
    bool hasPowers;

    [ObservableProperty]
    bool hasItems;

    [ObservableProperty]
    bool hasStatus;

    [ObservableProperty]
    bool hasOfferings;

    [ObservableProperty]
    bool hasAddons;

    [ObservableProperty]
    ObservableCollection<string>? files;
}
