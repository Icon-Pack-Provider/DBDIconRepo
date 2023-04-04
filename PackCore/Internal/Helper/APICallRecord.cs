using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPack.Internal.Helper;

internal static class APICallRecord
{
    //Save the time on last call of SearchRepositoriesRequest
    const string LastSearchDateRecord = "lastSearchDate.txt";
    public static void SaveLastSearchDate()
    {
        var file = IOHelper.GetFile(LastSearchDateRecord);
        using var fs = new FileStream(file.FullName, file.Exists ? FileMode.Open : FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var writer = new StreamWriter(fs);
        writer.Write(DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture.DateTimeFormat));
    }

    public static void DeleteSearchDateRecord()
    {
        var file = IOHelper.GetFile(LastSearchDateRecord);
        if (file.Exists)
            file.Delete();
    }

    public static bool ShouldDoSearch()
    {
        var file = IOHelper.GetFile(LastSearchDateRecord);
        if (!file.Exists)
            return true;
        string timeSTR = File.ReadAllText(file.FullName);
        bool parsed = DateTime.TryParseExact(timeSTR, "G", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out DateTime parsedTime);
        if (!parsed)
            return true;
        return parsedTime < (DateTime.UtcNow.AddHours(18));
    }


}
