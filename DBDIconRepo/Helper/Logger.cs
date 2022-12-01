using DBDIconRepo.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBDIconRepo.Helper
{
    public static class Logger
    {
        public static void Write(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            string logFile = Path.Join(SettingManager.Instance.CacheAndDisplayDirectory, "error.log");
            if (!File.Exists(logFile))
            {
                File.WriteAllText(logFile, "");
            }
            File.AppendAllText(logFile, $"\r\n{DateTime.Now:G}\r\n{message}");
        }
    }
}
