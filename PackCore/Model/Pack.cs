
using IconPack.Helper;
using Octokit;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

#nullable enable
namespace IconPack.Model
{
    public class Pack : Observable
    {
        string? _name;
        public string? Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        string? _description;
        public string? Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        ObservableCollection<string>? _author;
        public ObservableCollection<string>? Authors
        {
            get => _author;
            set => Set(ref _author, value);
        }

        string? _url;
        public string? URL
        {
            get => _url;
            set => Set(ref _url, value);
        }

        DateTime _lastUpdate;
        [JsonConverter(typeof(DateTimeConversion))]
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set => Set(ref _lastUpdate, value);
        }

        PackRepositoryInfo? _repository;
        public PackRepositoryInfo? Repository
        {
            get => _repository;
            set => Set(ref _repository, value);
        }

        PackContentInfo? _contentInfo;
        public PackContentInfo ContentInfo
        {
            get => _contentInfo;
            set => Set(ref _contentInfo, value);
        }
    }

    public partial class PackRepositoryInfo : Observable
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

        string? _name;
        public string? Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        long _id;
        public long ID
        {
            get => _id;
            set => Set(ref _id, value);
        }

        string? _owner;
        public string? Owner
        {
            get => _owner;
            set => Set(ref _owner, value);
        }

        string? _defaultBranch;
        public string? DefaultBranch
        {
            get => _defaultBranch;
            set => Set(ref _defaultBranch, value);
        }


        string? _cloneUrl;
        public string? CloneUrl
        {
            get => _cloneUrl;
            set => Set(ref _cloneUrl, value);
        }
    }

    public partial class PackContentInfo : Observable
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

        bool _hasPerks;
        public bool HasPerks
        {
            get => _hasPerks;
            set => Set(ref _hasPerks, value);
        }

        bool _hasPortraits;
        public bool HasPortraits
        {
            get => _hasPortraits;
            set => Set(ref _hasPortraits, value);
        }


        bool _hasPowers;
        public bool HasPowers
        {
            get => _hasPowers;
            set => Set(ref _hasPowers, value);
        }

        bool _hasItems;
        public bool HasItems
        {
            get => _hasItems;
            set => Set(ref _hasItems, value);
        }

        bool _hasStatus;
        public bool HasStatus
        {
            get => _hasStatus;
            set => Set(ref _hasStatus, value);
        }

        bool _hasOfferings;
        public bool HasOfferings
        {
            get => _hasOfferings;
            set => Set(ref _hasOfferings, value);
        }

        bool _hasAddons;
        public bool HasAddons
        {
            get => _hasAddons;
            set => Set(ref _hasAddons, value);
        }

        ObservableCollection<string>? _files;
        public ObservableCollection<string>? Files
        {
            get => _files;
            set => Set(ref _files, value);
        }
    }
}
#nullable disable