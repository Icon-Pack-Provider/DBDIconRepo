using IconPack.Model;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Console.WriteLine("Enter pack folder:");
string? path = Console.ReadLine();
string workingDirectory = string.IsNullOrWhiteSpace(path) ? Environment.CurrentDirectory : path;
if (!File.Exists(".gitignore"))
{
    try
    {
        if (Environment.CurrentDirectory == workingDirectory)
        {
            using StreamWriter writer = File.CreateText($"{workingDirectory}\\.gitignore");
            writer.WriteLine($"{Process.GetCurrentProcess().ProcessName}.exe");
        }
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine("Can't find that folder\r\n" +
            "If that folder actually exist, check Windows Security > Virus & threat protection > Manage Controlled folder access > Allow an app through Controllled folder access");
        return;
    }
}

string packJson = $"{workingDirectory}\\pack.json";
if (File.Exists(packJson))
{
    File.Delete(packJson);
}

DirectoryInfo info = new(workingDirectory);
List<string> fileList = info.GetFiles("*", SearchOption.AllDirectories).Where(file => file.Extension == ".png").Select(info => info.FullName).ToList();
for (int i = 0; i < fileList.Count; i++)
{
    //Remove path, only leaving with SubDirectory\Filename
    fileList[i] = fileList[i].Replace(workingDirectory, "");
}
Pack newInfo = new()
{
    Name = info.Name.Replace("-", " "),
    Description = null,
    Author = null,
    LastUpdate = DateTime.UtcNow,
    URL = null,
    ContentInfo = new PackContentInfo()
    {
        Files = new(fileList),
        HasAddons = Directory.Exists($"{workingDirectory}\\ItemAddons"),
        HasItems = Directory.Exists($"{workingDirectory}\\items"),
        HasOfferings = Directory.Exists($"{workingDirectory}\\Favors"),
        HasPerks = Directory.Exists($"{workingDirectory}\\Perks"),
        HasPortraits = Directory.Exists($"{workingDirectory}\\CharPortraits"),
        HasPowers = Directory.Exists($"{workingDirectory}\\Powers"),
        HasStatus = Directory.Exists($"{workingDirectory}\\StatusEffects")
    }
};

fillinfo:
Console.WriteLine("Do you want to fill the rest of the info? (Description, URL, or Author name) [Y/N]");
string? answer = Console.ReadLine();
if (answer == "Y")
{
    Console.WriteLine("What info do you want to fill?");
    Console.WriteLine("1. Description");
    Console.WriteLine("2. Author name");
    Console.WriteLine("3. URL");

    int.TryParse(Console.ReadLine(), out int index);
    switch (index)
    {
        case 1:
            Console.WriteLine("Enter a short description:");
            newInfo.Description = Console.ReadLine();
            break;
        case 2:
            Console.WriteLine("Enter custom author name");
            newInfo.Author = Console.ReadLine();
            break;
        case 3:
            Console.WriteLine("Enter git URL of the pack project (eg. https://github.com/Icon-Pack-Provider/Dead-by-daylight-Default-icons");
            newInfo.URL = Console.ReadLine();
            break;
    }
    goto fillinfo;
}

string output = System.Text.Json.JsonSerializer.Serialize(newInfo, new JsonSerializerOptions()
{
    WriteIndented = true,
    IncludeFields = false,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
});

using (StreamWriter writer = File.CreateText(packJson))
{
    writer.Write(output);
}