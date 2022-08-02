﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPackAPI.Internal
{
    /// <summary>
    /// For folders that have subfolders in it
    /// </summary>
    internal interface IFolder
    {
        #nullable enable
        /// <summary>
        /// If the folder field is null, that's mean it was in a folder, not under DLC folder
        /// </summary>
        string? Folder { get; set; }
        #nullable disable
    }
}