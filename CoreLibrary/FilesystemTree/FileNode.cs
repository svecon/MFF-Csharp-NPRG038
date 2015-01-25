using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.FilesystemTree
{
    public class FileNode : AbstractNode, IFilesystemTreeFileNode
    {
        public IFilesystemTreeDirNode ParentNode { get; set; }

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
            return ParentNode.GetAbsolutePath(location) + @"\" + Info.Name;
        }
    }
}