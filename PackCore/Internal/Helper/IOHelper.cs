using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IconPack.Internal.Flags;
using static IconPack.Resource.Terms;

namespace IconPack.Internal.Helper;

/// <summary>
/// Work with local files
/// </summary>
internal static class IOHelper
{
    #region CLONE/CACHE, actual files
    public static DirectoryInfo GetCloneDirectory()
    {
        string path = Path.Combine(WorkingDirectory, CloneDirectory);
        if (Directory.Exists(path))
            Directory.CreateDirectory(path);
        return new DirectoryInfo(path);
    }

    public static DirectoryInfo GetRepoCloneDirectory(string owner, string repo)
    {
        var folder = new DirectoryInfo(Path.Combine(GetCloneDirectory().FullName, owner, repo));
        if (!folder.Exists)
            folder.Create();
        return folder;
    }

    public static bool IsRepoCloneDirectoryDotGitExist(string owner, string repo)
    {
        var folder = GetRepoCloneDirectory(owner, repo);
        string dotGit = Path.Combine(folder.FullName, ".git");
        return Directory.Exists(dotGit);
    }

    public static FileInfo GetRepoFetchHead(string owner, string repo)
    {
        var cloneDir = GetRepoCloneDirectory(owner, repo);
        string path = Path.Combine(cloneDir.FullName, ".git");
        path = Path.Join(path, Terms.LastFetchFilename);
        return new(path);
    }

    public static FileInfo GetFile(string name)
    {
        return new FileInfo(Path.Join(WorkingDirectory, name));
    }
    #endregion
    #region Cache overload
    public static DirectoryInfo GetRepoCloneDirectory(PackRepositoryInfo repository)
        => GetRepoCloneDirectory(repository.Owner, repository.Name);
    public static DirectoryInfo GetRepoCloneDirectory(Octokit.Repository repository)
        => GetRepoCloneDirectory(repository.Owner.Login, repository.Name);

    public static bool IsRepoCloneDirectoryDotGitExist(PackRepositoryInfo repository)
        => IsRepoCloneDirectoryDotGitExist(repository.Owner, repository.Name);
    public static bool IsRepoCloneDirectoryDotGitExist(Octokit.Repository repository)
        => IsRepoCloneDirectoryDotGitExist(repository.Owner.Login, repository.Name);

    public static FileInfo GetRepoFetchHead(PackRepositoryInfo repository)
        => GetRepoFetchHead(repository.Owner, repository.Name);
    public static FileInfo GetRepoFetchHead(Octokit.Repository repository)
        => GetRepoFetchHead(repository.Owner.Login, repository.Name);
    #endregion


    #region DISPLAY/(use in UI, temporal, placeholders, thumbnails, nobanner or noreadme marker)
    /// <summary>
    /// Return only the display path
    /// There should be a bunch of repo owner folders in there
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo GetDisplayDirectory()
    {
        string path = Path.Combine(WorkingDirectory, DisplayDirectory);
        if (Directory.Exists(path))
            Directory.CreateDirectory(path);
        return new DirectoryInfo(path);
    }

    /// <summary>
    /// Return the folder use to store repo files in there. Like "NOBANNER" or "pack.json"
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="repoName"></param>
    /// <returns></returns>
    public static DirectoryInfo GetRepoDisplayDirectory(string owner, string repoName)
    {
        //eg:C:\Working\Display\Owner\Name
        string path = Path.Combine(WorkingDirectory, DisplayDirectory, owner, repoName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return new DirectoryInfo(path);
    }


    public static FileInfo GetRepoContentDisplayDirectory(string owner, string repoName, string filePath)
    {
        if (filePath.Contains('/'))
            filePath = filePath.Replace('/', '\\');

        return new FileInfo(Path.Join(WorkingDirectory, DisplayDirectory, owner, repoName, filePath));
    }

    public static FileInfo GetLocalPackJson(string owner, string repoName) 
        => GetRepoContentDisplayDirectory(owner, repoName, PackJson);

    public static bool IsLocalPackJSONExist(string owner, string repoName)
    {
        var file = GetRepoContentDisplayDirectory(owner, repoName, PackJson);
        if (!Directory.Exists(file.DirectoryName))
            Directory.CreateDirectory(file.DirectoryName);
        return file.Exists;
    }
    public static FileInfo GetLocalReadme(string owner, string repoName)
    => GetRepoContentDisplayDirectory(owner, repoName, ReadmeFile);
    #endregion
    #region Display overload
    public static DirectoryInfo GetRepoDisplayDirectory(PackRepositoryInfo repository) =>
        GetRepoDisplayDirectory(repository.Owner, repository.Name);

    public static DirectoryInfo GetRepoDisplayDirectory(Octokit.Repository repository) =>
        GetRepoDisplayDirectory(repository.Owner.Login, repository.Name);

    public static FileInfo GetRepoContentDisplayDirectory(PackRepositoryInfo repository, string filePath)
        => GetRepoContentDisplayDirectory(repository.Owner, repository.Name, filePath);

    public static FileInfo GetRepoContentDisplayDirectory(Octokit.Repository repository, string filePath)
        => GetRepoContentDisplayDirectory(repository.Owner.Login, repository.Name, filePath);


    public static FileInfo GetLocalPackJson(PackRepositoryInfo repository)
        => GetRepoContentDisplayDirectory(repository, PackJson);

    public static FileInfo GetLocalPackJson(Octokit.Repository repository)
        => GetRepoContentDisplayDirectory(repository, PackJson);

    public static bool IsLocalPackJSONExist(PackRepositoryInfo repository)
        => IsLocalPackJSONExist(repository.Owner, repository.Name);

    public static bool IsLocalPackJSONExist(Octokit.Repository repository)
        => IsLocalPackJSONExist(repository.Owner.Login, repository.Name);

    public static FileInfo GetLocalReadme(PackRepositoryInfo repository)
        => GetLocalReadme(repository.Owner, repository.Name);

    public static FileInfo GetLocalReadme(Octokit.Repository repository)
        => GetLocalReadme(repository.Owner.Login, repository.Name);
    #endregion
}
