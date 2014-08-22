﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FilesystemCrawler.Interfaces
{
    public interface IFilesystemTreeAbstractNode
    {
        FilesystemCrawler.Enums.NodeStatus Status { get; set; }
    }
}
