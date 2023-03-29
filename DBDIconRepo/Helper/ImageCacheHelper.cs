using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DBDIconRepo.Model;
using OneOf;
using System.Threading;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Helper;

//Based on https://github.com/floydpink/CachedImage/blob/main/source/FileCache.cs
/// <summary>
/// Every input URL send, return Local File path of that image instead
/// </summary>
public static class ImageCacheHelper
{        
    // Record whether a file is being written.
    private static readonly Dictionary<string, bool> IsWritingFile = new Dictionary<string, bool>();

    // HttpClient is intended to be instantiated once per application, rather than per-use.
    private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() => new HttpClient());

    // default cache directory - can be changed if needed from App.xaml
    private static readonly string AppCacheDirectory = $"{SettingManager.Instance.CacheAndDisplayDirectory}\\{"WebImageCache"}\\";

    private static readonly SemaphoreSlim WaitForHTTPClient = new SemaphoreSlim(1);

    private static async Task<OneOf<string, CacheState>> HitAsync(string url)
    {
        await WaitForHTTPClient.WaitAsync();
        if (!Directory.Exists(AppCacheDirectory))
        {
            Directory.CreateDirectory(AppCacheDirectory);
        }

        var uri = new Uri(url);
        var fileName = HashURIToFilename(uri);
        var localFile = Path.Combine(AppCacheDirectory, fileName);

        if (!IsWritingFile.ContainsKey(fileName) && File.Exists(localFile))
        {
            return localFile;
        }

        var client = LazyHttpClient.Value;
        FileStream? fileStream = null;
        try
        {

            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode is false)
            {
                return CacheState.NotExisting;
            }

            if (!IsWritingFile.ContainsKey(fileName))
            {
                IsWritingFile[fileName] = true;
                fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
            }

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var bytebuffer = new byte[100];
                int bytesRead;
                do
                {
                    bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 100);
                    if (fileStream != null)
                    {
                        await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                    }
                } while (bytesRead > 0);

                if (fileStream != null)
                {
                    await fileStream.FlushAsync();
                    fileStream.Dispose();
                    IsWritingFile.Remove(fileName);
                }
            }

            return localFile;
        }
        catch (TaskCanceledException tce) when (tce.InnerException is TimeoutException)
        {
            return CacheState.Timeout;
        }
        catch (WebException)
        {
            return CacheState.Error;
        }
        finally
        {
            Messenger.Default.Send(new AttemptReloadIconMessage(url), MessageToken.AttemptReloadIconMessage);
            WaitForHTTPClient.Release();
        }
    }

    private static string? SearchLocalClonedCache(string url)
    {
        var repoInfo = URL.ExtractRepositoryInfoFromURL(url);

        StringBuilder bd = new(SettingManager.Instance.CacheAndDisplayDirectory);
        if (bd[bd.Length - 1] != '\\')
            bd.Append('\\');
        bd.Append("Clone");
        bd.Append('\\');
        bd.Append(repoInfo.user);
        bd.Append('\\');
        bd.Append(repoInfo.repo);
        foreach (var path in repoInfo.paths)
        {
            bd.Append('\\');
            bd.Append(path);
        }
        if (File.Exists(bd.ToString()))
            return bd.ToString();
        return null;
    }

    private static string? SearchLocalUploadCache(string url)
    {
        var repoInfo = URL.ExtractRepositoryInfoFromURL(url);

        StringBuilder bd = new(SettingManager.Instance.CacheAndDisplayDirectory);
        if (bd[bd.Length - 1] != '\\')
            bd.Append('\\');
        bd.Append("Upload");
        bd.Append('\\');
        bd.Append(repoInfo.user);
        bd.Append('\\');
        bd.Append(repoInfo.repo);
        foreach (var path in repoInfo.paths)
        {
            bd.Append('\\');
            bd.Append(path);
        }
        if (File.Exists(bd.ToString()))
            return bd.ToString();
        return null;
    }

    private static string HashURIToFilename(Uri uri)
    {
        var fileNameBuilder = new StringBuilder();
        using (var sha256 = SHA256.Create())
        {
            var canonicalUrl = uri.ToString();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
            fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower());
            if (Path.HasExtension(canonicalUrl))
                fileNameBuilder.Append(Path.GetExtension(canonicalUrl).Split('?')[0]);
        }
        return fileNameBuilder.ToString();
    }

    private static string HashURIToFilename(string url) => HashURIToFilename(new Uri(url));

    /// <summary>
    /// Return local path
    /// </summary>
    /// <param name="url">Web image</param>
    /// <returns></returns>
    public static async Task<string> GetImage(string url)
    {
        //Check from cloned repo cache
        if (SearchLocalClonedCache(url) is string localClonedPath)
            return localClonedPath;
        if (SearchLocalUploadCache(url) is string localUploadPath)
            return localUploadPath;

        var filePath = HashURIToFilename(url);
        var localFile = Path.Combine(AppCacheDirectory, filePath);
        if (File.Exists(localFile))
        {
            return localFile;
        }
        var retry = await HitAsync(url);
        if (retry.TryPickT0(out string result, out CacheState remainder))
        {
            return result;
        }
        else
        {
            if (remainder == CacheState.NotExisting ||
                remainder == CacheState.Error)
                return "TIMEOUT";
        }
        return localFile;
    }

    public enum CacheState
    {
        NotExisting,
        Timeout,
        Caching,
        Cached,
        Error
    }
}
