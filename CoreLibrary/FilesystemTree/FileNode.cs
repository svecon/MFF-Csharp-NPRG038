using System;
using System.Diagnostics;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    /// <summary>
    /// File node representing a file in multiple locations.
    /// </summary>
    public class FileNode : AbstractNode, IFilesystemTreeFileNode
    {
        public IFilesystemTreeDirNode ParentNode { get; set; }

        /// <summary>
        /// Construtor for FileNode.
        /// </summary>
        /// <param name="parentNode">Parent DirNode for this node.</param>
        /// <param name="info">File info for this node.</param>
        /// <param name="location">Location where this node has been found from.</param>
        /// <param name="mode">Default diff mode.</param>
        public FileNode(IFilesystemTreeDirNode parentNode, FileInfo info, LocationEnum location, DiffModeEnum mode)
            : base(info, location, mode)
        {
            ParentNode = parentNode;
        }

        public override void Accept(IFilesystemTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string GetAbsolutePath(LocationEnum location)
        {
            if (ParentNode != null)
                return ParentNode.GetAbsolutePath(location) + @"\" + Info.Name;
            
            switch (location)
            {
                case LocationEnum.OnBase:
                    return InfoBase.FullName;
                case LocationEnum.OnLocal:
                    return InfoLocal.FullName;
                case LocationEnum.OnRemote:
                    return InfoRemote.FullName;
            }

            throw new ArgumentException("There is no path for given location");
        }
    }
}