using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.TwoWay;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.Processors.Postprocessors
{
    /// <summary>
    /// Processor for merging 2-way diffed files.
    /// 
    /// There must be an OutputFolder specified.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 300, DiffModeEnum.TwoWay)]
    public class MergeTwoWayProcessor : ProcessorAbstract
    {
        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        [Settings("Create empty folders.", "empty-folders", "Ef")]
        public bool CreateEmptyFolders = false;

        public enum DefaultActionEnum
        {
            WriteConflicts, RevertToLocal, ApplyRemote
        }

        [Settings("Default action for merging files.", "2merge-default", "2d")]
        public DefaultActionEnum DefaultAction;

        protected override bool CheckStatus(IFilesystemTreeDirNode node)
        {
            return base.CheckStatus(node) && OutputFolder != null && CreateEmptyFolders;
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            return base.CheckStatus(node) && OutputFolder != null;
        }

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
            // create directory when file is created
            // this means that empty folders need to be created here

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            CheckAndCreateDirectory(string.Join("/", OutputFolder, node.RelativePath));
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            CheckAndCreateDirectory(node);
            node.Status = NodeStatusEnum.WasMerged;

            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnLocalRemote
                || dnode.Diff == null)
            {
                ((FileInfo)node.Info).CopyTo(CreatePath(node), true);
                return;
            }

            // create temporary file if the target file exists
            string temporaryPath;
            bool isTemporary = false;
            if (File.Exists(CreatePath(node)))
            {
                temporaryPath = CreatePath(node) + ".temp";
                isTemporary = true;
            } else
            {
                temporaryPath = CreatePath(node);
            }

            using (StreamReader localStream = ((FileInfo)node.InfoLocal).OpenText())
            using (StreamReader remoteStream = ((FileInfo)node.InfoRemote).OpenText())
            using (StreamWriter writer = File.CreateText(temporaryPath))
            {
                int n = 0;
                int m = 0;

                foreach (DiffItem diff in dnode.Diff.Items)
                {
                    // change default action depending on processor settings
                    if (diff.Action == DiffItem.ActionEnum.Default)
                    {
                        switch (DefaultAction)
                        {
                            case DefaultActionEnum.WriteConflicts:
                                // keep default
                                break;
                            case DefaultActionEnum.RevertToLocal:
                                diff.Action = DiffItem.ActionEnum.RevertToLocal;
                                break;
                            case DefaultActionEnum.ApplyRemote:
                                diff.Action = DiffItem.ActionEnum.ApplyRemote;
                                break;
                        }
                    }

                    // same
                    for (; n < diff.LocalLineStart; n++) { writer.WriteLine(localStream.ReadLine()); }
                    for (; m < diff.RemoteLineStart; m++) { remoteStream.ReadLine(); }


                    if (diff.Action == DiffItem.ActionEnum.Default)
                    {
                        writer.WriteLine("<<<<<<< " + dnode.InfoLocal.FullName);
                        node.Status = NodeStatusEnum.HasConflicts;
                    }

                    // deleted
                    for (int p = 0; p < diff.LocalAffectedLines; p++)
                    {
                        if (diff.Action == DiffItem.ActionEnum.RevertToLocal
                            || diff.Action == DiffItem.ActionEnum.Default)
                        {
                            writer.WriteLine(localStream.ReadLine());
                        } else
                        {
                            localStream.ReadLine();
                        }
                        n++;
                    }

                    if (diff.Action == DiffItem.ActionEnum.Default)
                        writer.WriteLine("=======");

                    // inserted
                    for (int p = 0; p < diff.RemoteAffectedLines; p++)
                    {
                        if (diff.Action == DiffItem.ActionEnum.ApplyRemote
                            || diff.Action == DiffItem.ActionEnum.Default)
                        {
                            writer.WriteLine(remoteStream.ReadLine());
                        }

                        m++;
                    }

                    if (diff.Action == DiffItem.ActionEnum.Default)
                        writer.WriteLine(">>>>>>> " + dnode.InfoRemote.FullName);
                }

                // same
                for (; n < dnode.Diff.FilesLineCount.Local; n++) { writer.WriteLine(localStream.ReadLine()); }
                //for (; m < dnode.Diff.FilesLineCount.Remote; m++) { remoteStream.ReadLine(); }
            }

            // copy temporary file to correct location
            if (!isTemporary) return;

            File.Delete(CreatePath(node));
            File.Move(temporaryPath, CreatePath(node));
        }

        private void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void CheckAndCreateDirectory(IFilesystemTreeFileNode node)
        {
            CheckAndCreateDirectory(CreatePath(node, false));
        }

        private string CreatePath(IFilesystemTreeFileNode node, bool includeFileName = true)
        {
            string output = node.ParentNode == null || (node.ParentNode != null && node.ParentNode.RelativePath == "")
                ? OutputFolder
                : Path.Combine(OutputFolder, node.ParentNode.RelativePath);

            if (includeFileName)
                output = Path.Combine(output, node.Info.Name);

            return output;
        }
    }
}
