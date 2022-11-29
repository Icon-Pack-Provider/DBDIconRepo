using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper
{
    public static class ProcessChecker
    {
        public static readonly string[] DBDProcesses = 
        {
            "DeadByDaylight",
            "DeadByDaylight-Win64-Shipping"
        };

        public static bool IsDBDRunning()
        {
            var process = Process.GetProcessesByName(DBDProcesses[0]);
            if (process.Length > 0)
                return true;

            process = Process.GetProcessesByName(DBDProcesses[1]);
            if (process.Length > 0) 
                return true;

            return false;
        }
    }
}
