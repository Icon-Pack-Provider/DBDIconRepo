using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;
using DBDIconRepo.Strings;

namespace DBDIconRepo.Service;

public class SecureSettingService
{
    private const string SettingFilename = "LOGIN";
    private const string Salt = "SALT";
    private static FileInfo GetLoginFile(string filename)
    {
        StringBuilder pathBuilder = new();
        pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        pathBuilder.Append('\\');
#if DEBUG
        pathBuilder.Append(Terms.AppDataFolder);
        pathBuilder.Append("Dev");
        pathBuilder.Append('\\');
#else
        pathBuilder.Append(Terms.AppDataFolder);
        pathBuilder.Append('\\');
#endif
        pathBuilder.Append(filename);
        var file = new FileInfo(pathBuilder.ToString());
        if (!file.Directory.Exists)
            file.Directory.Create();
        return file;
    }

    public string? GetSecurePassword()
    {
        if (!OperatingSystem.IsWindows())
            return string.Empty;
        var file = GetLoginFile(SettingFilename);
        var salt = GetLoginFile(Salt);
        if (!file.Exists || !salt.Exists)
            return null;
        var saltBytes = ReadFromFile(salt);
        var encrypted = ReadFromFile(file);

        var decrypted = ProtectedData.Unprotect(encrypted, saltBytes, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decrypted);
    }

    public void SaveSecurePassword(string password)
    {
        if (!OperatingSystem.IsWindows())
            return;
        var file = GetLoginFile(SettingFilename);
        var salt = GetLoginFile(Salt);

        var bytes = Encoding.UTF8.GetBytes(password);

        var generatedSalt = RandomNumberGenerator.GetBytes(69);
        SaveToFile(salt, generatedSalt);

        
        var encrypted = ProtectedData.Protect(bytes, generatedSalt, DataProtectionScope.CurrentUser);
        SaveToFile(file, encrypted);
    }

    public void Logout()
    {
        var file = GetLoginFile(SettingFilename);
        file.Delete();
        var salt = GetLoginFile(Salt);
        salt.Delete();
    }

    private byte[] ReadFromFile(FileInfo file)
    {
        string path = file.FullName;
        return File.ReadAllBytes(path);
    }

    private void SaveToFile(FileInfo file, byte[] input)
    {
        if (file.Exists)
            file.Delete();
        File.WriteAllBytes(file.FullName, input);
    }
}
