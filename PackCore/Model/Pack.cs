using CommunityToolkit.Mvvm.ComponentModel;
using Octokit;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace IconPack.Model
{
    public partial class Pack : ObservableObject
    {
        [ObservableProperty]
        string? _name;

        [ObservableProperty]
        string? _description;

        [ObservableProperty]
        string? _author;

        [ObservableProperty]
        string? _url;
        public string? URL
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        DateTime _lastUpdate;
        [JsonConverter(typeof(Helper.DateTimeConversion))]
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set => SetProperty(ref _lastUpdate, value);
        }

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

        long _id;
        public long ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

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
