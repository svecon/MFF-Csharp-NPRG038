﻿using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// DiffCrawler returns instance of FilesystemDiffTree
    /// </summary>
    public class DiffCrawler : Crawler
    {
        protected override INode CreateFilesystemTree(DiffModeEnum mode)
        {
            return new FilesystemDiffTree(mode);
        }
    }
}
