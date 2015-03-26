using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Postprocessors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm.Diff;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.Processors.Postprocessors
{
    /// <summary>
    /// TODO
    /// </summary>
    public class MergeTwoWayProcessor : PostProcessorAbstract
    {
        public override int Priority { get { return 300; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        [Settings("Merge folders and files.", "merge", "m")]
        public bool IsEnabled = false;

        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        [Settings("Create empty folders.", "empty-folders", "Ef")]
        public bool CreateEmptyFolders = false;

        public override void Process(IFilesystemTreeDirNode node)
        {
            // create directory when file is created

            // this means that empty folders need to be created here

            if (!CheckModeAndStatus(node))
                return;

            if (OutputFolder == null)
                IsEnabled = false;

            if (!IsEnabled)
                return;

            // processor setting
            if (!CreateEmptyFolders)
                return;

            // if there are any files, folder will be created implicitly
            if (node.Files.Count > 0)
                return;

            // otherwise create empty folder
            CheckAndCreateDirectory(string.Join("/", OutputFolder, node.RelativePath));
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (!IsEnabled)
                return;

            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            if (dnode.Diff == null)
            {
                CheckAndCreateDirectory(node);
                ((FileInfo) node.Info).CopyTo(CreatePath(node), true);
                node.Status = NodeStatusEnum.WasMerged;
                return;
            }

            using (StreamReader localStream = ((FileInfo)node.InfoLocal).OpenText())
            using (StreamReader remoteStream = ((FileInfo)node.InfoRemote).OpenText())
            using (StreamWriter writer = File.CreateText(CreatePath(node)))
            {
                int n = 0;
                int m = 0;

                foreach (DiffItem diff in dnode.Diff.Items)
                {
                    // same
                    for (; n < diff.OldLineStart; n++) { writer.WriteLine(localStream.ReadLine()); }
                    for (; m < diff.NewLineStart; m++) { remoteStream.ReadLine(); }


                    if (diff.Action == DiffItemActionEnum.Default)
                        writer.WriteLine("<<<<<<< " + dnode.InfoLocal.Name);

                    // deleted
                    for (int p = 0; p < diff.DeletedInOld; p++)
                    {
                        if (diff.Action == DiffItemActionEnum.RevertToLocal
                            || diff.Action == DiffItemActionEnum.Default)
                        {
                            writer.WriteLine(localStream.ReadLine());
                        }
                        else
                        {
                            localStream.ReadLine();
                        }
                        n++;
                    }

                    if (diff.Action == DiffItemActionEnum.Default)
                        writer.WriteLine("=======");

                    // inserted
                    for (int p = 0; p < diff.InsertedInNew; p++)
                    {
                        if (diff.Action == DiffItemActionEnum.ApplyRemote
                            || diff.Action == DiffItemActionEnum.Default)
                        {
                            writer.WriteLine(remoteStream.ReadLine());
                        }
                        
                        m++;
                    }

                    if (diff.Action == DiffItemActionEnum.Default)
                        writer.WriteLine(">>>>>>> " + dnode.InfoRemote.Name);
                }

                // same
                for (; n < dnode.Diff.FilesLineCount.Local; n++) { writer.WriteLine(localStream.ReadLine()); }
                //for (; m < dnode.Diff.FilesLineCount.Remote; m++) { remoteStream.ReadLine(); }
            }

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
                ? string.Join("/", OutputFolder, node.Info.Name)
                : string.Join("/", OutputFolder, node.ParentNode.RelativePath, node.Info.Name);

            if (includeFileName)
                output = string.Join("/", output, node.Info.Name);

            return output;
        }
    }
}
