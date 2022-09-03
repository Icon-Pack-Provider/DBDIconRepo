
using CommunityToolkit.Mvvm.ComponentModel;
using IconPack.Helper;
using Octokit;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

#nullable enable
namespace IconPack.Model
{
    public partial class Pack : ObservableObject
    {
        [ObservableProperty]
        string? _name;

        [ObservableProperty]
        string? _description;

        [ObservableProperty]
        ObservableCollection<string>? _authors;

        [ObservableProperty]
        string? _uRL;

        [ObservableProperty]
        DateTime _lastUpdate;

        [ObservableProperty]
        PackRepositoryInfo? _repository;

        [ObservableProperty]
        PackContentInfo? _contentInfo;
    }

    public partial class PackRepositoryInfo : ObservableObject
    {
        public PackRepositoryInfo() { }
        public PackRepositoryInfo(Repository repo)
        {
            ID = repo.Id;
            Name = repo.Name;
            Owner = repo.Owner.Login;
            DefaultBranch = repo.DefaultBranch;
            CloneUrl = repo.CloneUrl;
        }

        [ObservableProperty]
        string? _name;

        [ObservableProperty]
        long _iD;

        [ObservableProperty]
        string? _owner;

        [ObservableProperty]
        string? _defaultBranch;

        [ObservableProperty]
        string? _cloneUrl;
    }

    public partial class PackContentInfo : ObservableObject
    {
        public PackContentInfo() { }

        public static async Task<PackContentInfo> GetContentInfo(GitHubClient client, Repository repo)
        {
            var commits = await client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
            var head = commits[0];
            var treeContent = await client.Git.Tree.GetRecursive(repo.Id, head.Sha);
            var treeOnly = treeContent.Tree.Where(i => i.Type.Value == TreeType.Tree);

            PackContentInfo info = new()
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
            return info;
        }

        [ObservableProperty]
        bool _hasPerks;

        [ObservableProperty]
        bool _hasPortraits;

        [ObservableProperty]
        bool _hasPowers;

        [ObservableProperty]
        bool _hasItems;

        [ObservableProperty]
        bool _hasStatus;

        [ObservableProperty]
        bool _hasOfferings;

        [ObservableProperty]
        bool _hasAddons;

        [ObservableProperty]
        ObservableCollection<string>? _files;
    }
}
#nullable disable