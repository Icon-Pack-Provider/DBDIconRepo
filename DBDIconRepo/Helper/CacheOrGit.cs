using DBDIconRepo.Model;
using IconPack.Model;
using IconPack.Resource;
using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Helper;
{
    public static class CacheOrGit
    {
        private const string CachedFolderName = "Cache";
        private const string CachedDisplayName = "Display";
        private static GitHubClient client => Service.OctokitService.Instance.GitHubClientInstance;

        public static async Task<Pack?> GetPack(Repository repo)
        {
            //Directory
            string cachedRoot = GetDisplayContentPath(repo.Owner.Login, repo.Name);

            //Local pack.json info
            string packJsonCached = $"{cachedRoot}\\{Terms.PackJson}";
            if (!File.Exists(packJsonCached))
            {
                byte[] jsonRaw = new byte[] { };
                string json = "";
                //Get from git
                //Check if this repo actually have pack.json
                bool hasPackJson = await URL.IsContentExists(repo, Terms.PackJson);
                if (hasPackJson)
                {
                    try
                    {
                        jsonRaw = await client.Repository.Content.GetRawContent(repo.Owner.Login, repo.Name, Terms.PackJson);
                        //Then write to file as UTF-8
                        json = Encoding.UTF8.GetString(jsonRaw);
                    }
                    catch { }
                }
                //Try get pack.json on project
                
                //Fill in other missing details
                var packInfo = string.IsNullOrEmpty(json) ?
                    new Pack() :
                    JsonSerializer.Deserialize<Pack>(json);
                packInfo.Name = packInfo.Name ?? repo.Name;
                packInfo.Description = packInfo.Description ?? repo.Description;
                packInfo.Author = packInfo.Author ?? repo.Owner.Name ?? repo.Owner.Login;
                packInfo.URL = packInfo.URL ?? repo.HtmlUrl;
                packInfo.LastUpdate = repo.UpdatedAt.UtcDateTime;

                if (packInfo.Repository is null) { packInfo.Repository = new PackRepositoryInfo(repo); }
                if (packInfo.ContentInfo is null)
                {
                    packInfo.ContentInfo = await PackContentInfo.GetContentInfo(client, repo).ConfigureAwait(true);
                }

                //update
                json = JsonSerializer.Serialize(packInfo, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    IncludeFields = false
                });
                using (StreamWriter writer = File.CreateText(packJsonCached))
                {
                    writer.Write(json);
                }

                return packInfo;
            }
            else
            {
                //TODO:Consider here somewhere if file is too old, delete and get a new file instead

                //File has already cached
                using (var reader = File.OpenText(packJsonCached))
                {
                    string json = reader.ReadToEnd();

                    return JsonSerializer.Deserialize<Pack>(json);
                }
            }
        }

        public static async Task GatherPackDisplayData(Repository repo, Pack? requested, string[] requestedPreview)
        {
            //Directory
            string cachedRoot = GetDisplayContentPath(repo.Owner.Login, repo.Name);

            string readmePath = $"{cachedRoot}\\README.md";
            string noReadmeMarker = $"{cachedRoot}\\NOREADME";

            //Readme.md
            if (!File.Exists(readmePath) && !File.Exists(noReadmeMarker))
            {
                //Check if this repo have readme.md
                bool readmeState = await URL.IsContentExists(repo, "readme.md");
                bool READMEState = await URL.IsContentExists(repo, "README.md");
                //if so, get it
                if (readmeState || READMEState)
                {
                    //Get readme.md
                    var readme = await client.Repository.Content.GetReadme(repo.Id);
                    File.WriteAllText(readmePath, readme.Content);
                }
                else //otherwise, leave this empty file
                {
                    File.Create(noReadmeMarker);
                }
            }

            string bannerPath = $"{cachedRoot}\\.banner.png";
            string noBannerMarker = $"{cachedRoot}\\NOBANNER";

            //Banner existance
            if (!File.Exists($"{bannerPath}") && !File.Exists($"{noBannerMarker}")) 
            {
                //Get banner if it's exist
                bool bannerState = await URL.IsContentExists(repo, ".banner.png");
                if (bannerState)
                {
                    //Get banner
                    //var banner = await client.Repository.Content.GetRawContent(repo.Owner.Login, repo.Name, ".banner.png");
                    //using FileStream fs = new(path: $"{cachedRoot}\\.banner.png", mode: System.IO.FileMode.Create, FileAccess.Write);
                    //fs.Write(banner, 0, banner.Length);
                    //Make dummy banner to let "PackDisplay" know, it's exist on repo
                    File.Create(bannerPath);
                }
                else
                    File.Create(noBannerMarker);
            }
        }

        public static async Task<bool> DownloadItem(Pack? info, string path)
        {
            bool isExistOnGit = await URL.IsContentExists(info.Repository, path);
            if (!isExistOnGit)
                return false;

            //Download
            string localFile = GetPackDownloadPath(info.Repository.Owner, info.Repository.Name);
            if (path.Contains('/'))
                localFile += (path.StartsWith('/') ? "" : '\\') + path.Replace('/', '\\');
            else
                localFile += (path.StartsWith('\\') ? "" : '\\') + path;

            if (!File.Exists(path))
            {
                //Download
                var icon = await client.Repository.Content.GetRawContent(info.Repository.Owner, info.Repository.Name, path);
                using FileStream fs = new(localFile, System.IO.FileMode.Create, FileAccess.Write);
                fs.Write(icon, 0, icon.Length);
                return true;
            }
            else
                return true;
        }

        public static string GetDisplayContentPath(string owner, string name)
        {
            string path = $"{Environment.CurrentDirectory}\\{CachedFolderName}\\{CachedDisplayName}\\" +
                $"{owner}\\{name}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string GetContentPath(string owner, string name, string filePath)
        {
            return $"{Environment.CurrentDirectory}\\{CachedFolderName}\\" + 
                $"{owner}\\{name}{(filePath.StartsWith('/') ? string.Empty : '\\')}{filePath.Replace('/', '\\')}";
        }

        public static string GetPackDownloadPath(string owner, string name)
        {
            string path = $"{Environment.CurrentDirectory}\\{CachedFolderName}\\" +
                $"{owner}\\{name}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        //TODO:Deal with other pack type (Portrait, etc)
        private static Throttler _throttler = new(250);
        public static async Task DownloadPack(Repository repo, Pack pack)
        {
            string repoCachedFolder = $"{Environment.CurrentDirectory}\\{CachedFolderName}\\{repo.Owner.Login}\\{repo.Name}";

            var user = await client.User.Current();
            var token = client.Credentials.GetToken();

            if (!Directory.Exists(repoCachedFolder))
            {
                Directory.CreateDirectory(repoCachedFolder);
                //Start new download
                //pull from remote
                try
                {
                    Task.Run(() =>
                    {
                        LibGit2Sharp.Repository.Clone(pack.Repository.CloneUrl, repoCachedFolder, new LibGit2Sharp.CloneOptions()
                        {
                            CredentialsProvider = (_url, _user, _cred) => GetCredential(user.Login, token),
                            OnProgress = (progress) => ReportServerProgress(repo.Name, progress),
                            OnTransferProgress = (transfer) => ReportTransferProgress(repo.Name, transfer),
                            OnCheckoutProgress = (path, complete, total) => ReportCheckoutProgress(repo.Name, path, complete, total),
                            IsBare = false
                        });
                    }).Await();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Clone error!: {(ex is null ? "Unknown" : ex.Message)}");
                }
            }
            else
            {
                DirectoryInfo dir = new(repoCachedFolder);
                int count = dir.GetFiles("*", SearchOption.AllDirectories).Count();
                long size = dir.GetFiles("*", SearchOption.AllDirectories)
                    .Select(file => file.Length).Sum();


                //Get a latest commit date on both local and remote
                DateTime localLatestCommit = DateTime.MinValue;
                using (var localRepo = new LibGit2Sharp.Repository(repoCachedFolder))
                {
                    var latestCommit = localRepo.Commits.First();
                    localLatestCommit = latestCommit.Author.When.UtcDateTime;
                }

                //Geta latest commit date on remote
                DateTime remoteLatestCommit = DateTime.MinValue;

                var branches = await client.Repository.Branch.GetAll(repo.Id);
                var commit = await client.Repository.Commit.Get(repo.Id, branches.First().Commit.Sha);
                remoteLatestCommit = commit.Commit.Author.Date.UtcDateTime;

                if (localLatestCommit == remoteLatestCommit) //All downloaded and no new change?
                {
                    //Say it's done downloading
                    Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.Done, 1d),
                        $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN + repo.Name}");
                }
                else
                {
                    //New change on remote?
                    //Force reset folder
                    Task.Run(() =>
                    {
                        //Fetch change from remote
                        using (var libGitRepo = new LibGit2Sharp.Repository(repoCachedFolder))
                        {
                            //Reset everything
                            var originMaster = libGitRepo.Branches[$"origin/{repo.DefaultBranch}"];
                            libGitRepo.Reset(LibGit2Sharp.ResetMode.Hard, originMaster.Tip);
                        }
                    }).Await(() =>
                    {
                        //Report if it's done fetching
                        Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.Done, 1d),
                            $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN + repo.Name}");
                    });
                    return;
                }
            }
        }

        private static bool ReportServerProgress(string repositoryName, string progress)
        {
            //Counting objects:   0% (1/274)
            string[] progresses = progress.Split("()/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            double estimateProgress = 0;
            if (progresses.Length >= 3)
            {
                double cProg = -1;
                double total = -1;
                double.TryParse(progresses[1], out cProg);
                double.TryParse(progresses[2], out total);
                if (cProg >= 1 && total >= 1)
                    estimateProgress = cProg / total;
            }

            if (progress.StartsWith("Counting"))
            {
                _throttler.Throttle(() =>
                {
                    Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.Enumerating, estimateProgress),
                        $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{repositoryName}");
                });
            }
            else if (progress.StartsWith("Compressing"))
            {
                _throttler.Throttle(() =>
                {
                    Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.Compressing, estimateProgress),
                        $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{repositoryName}");
                });
            }

            System.Diagnostics.Debug.WriteLine($"{estimateProgress} ({progress})");
            return true;
        }

        private static bool ReportTransferProgress(string repositoryName, LibGit2Sharp.TransferProgress transfer)
        {
            if (transfer.ReceivedObjects < 1)
                return true;
            double estimate = Convert.ToDouble(transfer.ReceivedObjects.ToString()) / Convert.ToDouble(transfer.TotalObjects.ToString());
            _throttler.Throttle(() =>
            {
                Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.Transfering, estimate),
                    $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{repositoryName}");
            });
            System.Diagnostics.Debug.WriteLine($"{estimate}% ({transfer.ReceivedObjects}/{transfer.TotalObjects}");
            return true;
        }

        private static void ReportCheckoutProgress(string repositoryName, string checkingOutFile, int completedFileCount, int totalFileCount) 
        {
            double estimate = Convert.ToDouble(completedFileCount.ToString()) / Convert.ToDouble(totalFileCount.ToString());
            _throttler.Throttle(() =>
            {
                Messenger.Default.Send(new DownloadRepoProgressReportMessage(DownloadState.CheckingOut, estimate),
                    $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN + repositoryName}");
            });
            System.Diagnostics.Debug.WriteLine($"{estimate}% ({completedFileCount}/{totalFileCount})");
        }

        private static LibGit2Sharp.UsernamePasswordCredentials GetCredential(string username, string token)
        {
            return new LibGit2Sharp.UsernamePasswordCredentials()
            {
                Username = username,
                Password = token
            };
        }

        public static async Task<byte[]> LoadImage(string url)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
