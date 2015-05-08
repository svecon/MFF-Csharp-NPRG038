using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using TextDiffAlgorithm.TwoWay;

namespace TextDiffProcessors.MergeProcessors
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

        protected override bool CheckStatus(INodeDirNode node)
        {
            return base.CheckStatus(node) && OutputFolder != null && CreateEmptyFolders;
        }

        protected override bool CheckStatus(INodeFileNode node)
        {
            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            return base.CheckStatus(node) && OutputFolder != null;
        }

        protected override void ProcessChecked(INodeDirNode node)
        {
            // create directory when file is created
            // this means that empty folders need to be created here

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            CheckAndCreateDirectory(string.Join("/", OutputFolder, node.RelativePath));
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var dnode = node as FileDiffNode;

            if (dnode == null)
                return;

            // one file is missing
            if (node.Location < (int)LocationCombinationsEnum.OnLocalRemote)
            {
                FileInfo from;

                if (node.IsInLocation(LocationEnum.OnLocal))
                {
                    from = (FileInfo)node.InfoLocal;
                } else if (node.IsInLocation(LocationEnum.OnRemote))
                {
                    from = (FileInfo)node.InfoRemote;
                } else
                {
                    throw new InvalidDataException();
                }

                CheckAndCreateDirectory(Path.GetDirectoryName(CreatePath(node)));
                from.CopyTo(CreatePath(node), true);
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            // both files are present and are same
            if (dnode.Differences == DifferencesStatusEnum.AllSame)
            {
                File.Copy(dnode.InfoLocal.FullName, CreatePath(node), true);
            }

            // both files are present and action is set
            if (dnode.Action != PreferedActionThreeWayEnum.Default)
            {
                File.Copy(
                    dnode.Action == PreferedActionThreeWayEnum.ApplyLocal
                        ? dnode.InfoLocal.FullName
                        : dnode.InfoRemote.FullName, CreatePath(node), true);
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            var diff = dnode.Diff as Diff;

            if (diff == null)
                return;

            node.Status = NodeStatusEnum.WasMerged;

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

                foreach (DiffItem diffItem in diff.Items)
                {
                    // change default action depending on processor settings
                    if (diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                    {
                        switch (DefaultAction)
                        {
                            case DefaultActionEnum.WriteConflicts:
                                // keep default
                                break;
                            case DefaultActionEnum.RevertToLocal:
                                diffItem.PreferedAction = PreferedActionTwoWayEnum.ApplyLocal;
                                break;
                            case DefaultActionEnum.ApplyRemote:
                                diffItem.PreferedAction = PreferedActionTwoWayEnum.ApplyRemote;
                                break;
                        }
                    }

                    // same
                    for (; n < diffItem.LocalLineStart; n++) { writer.WriteLine(localStream.ReadLine()); }
                    for (; m < diffItem.RemoteLineStart; m++) { remoteStream.ReadLine(); }


                    if (diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                    {
                        writer.WriteLine("<<<<<<< " + dnode.InfoLocal.FullName);
                        node.Status = NodeStatusEnum.HasConflicts;
                    }

                    // deleted
                    for (int p = 0; p < diffItem.LocalAffectedLines; p++)
                    {
                        if (diffItem.PreferedAction == PreferedActionTwoWayEnum.ApplyLocal
                            || diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                        {
                            writer.WriteLine(localStream.ReadLine());
                        } else
                        {
                            localStream.ReadLine();
                        }
                        n++;
                    }

                    if (diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                        writer.WriteLine("=======");

                    // inserted
                    for (int p = 0; p < diffItem.RemoteAffectedLines; p++)
                    {
                        if (diffItem.PreferedAction == PreferedActionTwoWayEnum.ApplyRemote
                            || diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                        {
                            writer.WriteLine(remoteStream.ReadLine());
                        }

                        m++;
                    }

                    if (diffItem.PreferedAction == PreferedActionTwoWayEnum.Default)
                        writer.WriteLine(">>>>>>> " + dnode.InfoRemote.FullName);
                }

                // same
                for (; n < diff.FilesLineCount.Local; n++) { writer.WriteLine(localStream.ReadLine()); }
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

        private string CreatePath(INodeFileNode node)
        {
            string output = node.ParentNode == null
                    ? Path.Combine(OutputFolder, node.Info.Name)
                    : Path.Combine(OutputFolder, node.ParentNode.RelativePath, node.Info.Name);

            CheckAndCreateDirectory(Path.GetDirectoryName(output));

            return output;
        }
    }
}
