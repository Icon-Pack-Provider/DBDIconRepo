using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconRepository.String
{
    public static class Terms
    {
        public const string AppConfigFolder
#if DEBUG
            = "IconRepositoryDebug";
#else
            = "IconRepository";
#endif

        public const string ConfigFile = "config.json";

        /// <summary>
        /// Production head value for OctokitAPI
        /// </summary>
        public const string PHV = "IconRepository";
    }
}
