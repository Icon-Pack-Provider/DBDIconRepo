using CommunityToolkit.Mvvm.ComponentModel;
using IconInfo.Icon;
using IconPack.Helper;
using IconPack.Internal;
using IconPack.Internal.Helper;
using Octokit;
using System;
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
            if (Overrides is not null)
            {
                switch (Overrides.AuthorMode)
                {
                    case AuthorDisplayType.OwnerAndTop3Contributor:
                        if (Overrides.Authors.Count >= 4)
                            return $"{Repository.Owner}{string.Join(", ", Overrides.Authors.Take(3))}";
                        return $"{Repository.Owner}{string.Join(", ", Overrides.Authors)}";
                    case AuthorDisplayType.OwnerAndAllContributor:
                        return $"{Repository.Owner}{string.Join(", ", Overrides.Authors)}";
                    case AuthorDisplayType.OnlyOwner:
                        return Overrides.Authors.Count < 1 ? Repository.Owner : string.Join(", ", Overrides.Authors);
                }
            }
            if (!string.IsNullOrEmpty(author))
                return author;
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

        var commits = await OctokitService.Instance.Client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new ApiOptions()
        {
            PageCount = 1,
            PageSize = 1
        });
        var head = commits.First();
        var treeContent = await OctokitService.Instance.Client.Git.Tree.GetRecursive(repo.Id, head.Sha);
        var files = treeContent.Tree.Where(i => i.Type.Value == TreeType.Blob).Where(i => i.Path.EndsWith(".png") || i.Path.ToLower().Contains("readme.md")).Select(i => i.Path);

        PackContentInfo pcif = new()
        {
            Files = new(files)
        };
        pcif.VerifyContentInfo();

        return pcif;
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
    bool hasBanner;

    [ObservableProperty]
    bool hasReadme;

    [ObservableProperty]
    ObservableCollection<string?> files = new();

    public void VerifyContentInfo()
    {
        bool hasBanner = false, hasPerks = false, hasAddons = false, hasItems = false, hasOfferings = false, hasPortraits = false, hasPowers = false, hasStatus = false, hasReadme = false;
        
        Parallel.ForEach(Files, file =>
        {
            if (string.IsNullOrEmpty(file))
                return;
            if (string.Equals(file, "readme.md", StringComparison.OrdinalIgnoreCase))
            {
                hasReadme = true;
            }
            if (file.Contains(".banner") && !hasBanner)
            {
                hasBanner = true;
            }

            var info = IconInfo.Info.GetIcon(file);
            if (info is null)
            {
                return;
            }

            if (!hasPerks && info is Perk)
            {
                hasPerks = true;
            }

            if (!hasAddons && info is Addon)
            {
                hasAddons = true;
            }

            if (!hasItems && info is Item)
            {
                hasItems = true;
            }

            if (!hasOfferings && info is Offering)
            {
                hasOfferings = true;
            }

            if (!hasPortraits && info is Portrait)
            {
                hasPortraits = true;
            }

            if (!hasPowers && info is Power)
            {
                hasPowers = true;
            }

            if (!hasStatus && info is StatusEffect)
            {
                hasStatus = true;
            }
        });

        HasBanner = hasBanner;
        HasPerks = hasPerks;
        HasAddons = hasAddons;
        HasItems = hasItems;
        HasOfferings = hasOfferings;
        HasPortraits = hasPortraits;
        HasPowers = hasPowers;
        HasStatus = hasStatus;
    }
}
