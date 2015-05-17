using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using TextDiffAlgorithm.ThreeWay;

namespace TextDiffProcessors.MergeProcessors
{
    /// <summary>
    /// Processor for merging 3-way diffed files.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 301, DiffModeEnum.ThreeWay)]
    public class MergeThreeWayProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Setting for ouput folder of the merge.
        /// </summary>
        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        /// <summary>
        /// Default action used when the differences conflict.
        /// </summary>
        public enum DefaultActionEnum
        {
            /// <summary>
            /// Writes conflict, keeping all versions of the differences.
            /// </summary>
            WriteConflicts, 
            
            /// <summary>
            /// Revert to base version of the diff.
            /// </summary>
            RevertToBase, 
            
            /// <summary>
            /// Apply and keep only local version of the difference.
            /// </summary>
            ApplyLocal, 
            
            /// <summary>
            /// Apply and keep only remote version of the difference.
            /// </summary>
            ApplyRemote
        }

        /// <summary>
        /// Settings for the default action for the conflicts.
        /// </summary>
        [Settings("Default action for merging files.", "3merge-default", "3d")]
        public DefaultActionEnum DefaultAction;

        /// <inheritdoc />
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        /// <inheritdoc />
        protected override bool CheckStatus(INodeFileNode node)
        {
            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnAll3)
                return false;

            return base.CheckStatus(node);
        }

        /// <inheritdoc />
        protected override void ProcessChecked(INodeFileNode node)
        {
            var dnode = node as FileDiffNode;

            if (dnode == null)
                return;

            var diff = dnode.Diff as Diff3;

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

            using (StreamReader localStream = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader remoteStream = ((FileInfo)dnode.InfoRemote).OpenText())
            using (StreamReader baseStream = ((FileInfo)dnode.InfoBase).OpenText())
            using (StreamWriter writer = File.CreateText(temporaryPath))
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diff3Item in diff.Items)
                {
                    // change default action depending on processor settings
                    if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                    {
                        switch (DefaultAction)
                        {
                            case DefaultActionEnum.WriteConflicts:
                                // keep default
                                break;
                            case DefaultActionEnum.RevertToBase:
                                diff3Item.PreferedAction = PreferedActionThreeWayEnum.RevertToBase;
                                break;
                            case DefaultActionEnum.ApplyLocal:
                                diff3Item.PreferedAction = PreferedActionThreeWayEnum.ApplyLocal;
                                break;
                            case DefaultActionEnum.ApplyRemote:
                                diff3Item.PreferedAction = PreferedActionThreeWayEnum.ApplyRemote;
                                break;
                        }
                    }

                    // print between diffs
                    for (; o < diff3Item.BaseLineStart; o++) { writer.WriteLine(baseStream.ReadLine()); }
                    for (; m < diff3Item.LocalLineStart; m++) { localStream.ReadLine(); }
                    for (; n < diff3Item.RemoteLineStart; n++) { remoteStream.ReadLine(); }

                    // if there is an action asociated:
                    if (diff3Item.PreferedAction != PreferedActionThreeWayEnum.Default)
                    {
                        for (int p = 0; p < diff3Item.LocalAffectedLines; p++)
                        {
                            m++;

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.ApplyLocal)
                                writer.WriteLine(localStream.ReadLine());
                            else
                                localStream.ReadLine();
                        }
                        for (int p = 0; p < diff3Item.BaseAffectedLines; p++)
                        {
                            o++;

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.RevertToBase)
                                writer.WriteLine(baseStream.ReadLine());
                            else
                                baseStream.ReadLine();

                        }
                        for (int p = 0; p < diff3Item.RemoteAffectedLines; p++)
                        {
                            n++;

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.ApplyRemote)
                                writer.WriteLine(remoteStream.ReadLine());
                            else
                                remoteStream.ReadLine();
                        }

                        continue;
                    }

                    // print diffs
                    switch (diff3Item.Differeces)
                    {
                        case DifferencesStatusEnum.BaseLocalSame:
                            for (int p = 0; p < diff3Item.LocalAffectedLines; p++) { localStream.ReadLine(); m++; }
                            for (int p = 0; p < diff3Item.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff3Item.RemoteAffectedLines; p++) { writer.WriteLine(remoteStream.ReadLine()); n++; }
                            break;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            for (int p = 0; p < diff3Item.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff3Item.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff3Item.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            for (int p = 0; p < diff3Item.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff3Item.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff3Item.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.AllDifferent:

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                            {
                                node.Status = NodeStatusEnum.HasConflicts;
                                writer.WriteLine("<<<<<<< " + dnode.InfoLocal.FullName);
                            }


                            for (int p = 0; p < diff3Item.LocalAffectedLines; p++)
                            {
                                if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.ApplyLocal
                                    || diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(localStream.ReadLine());
                                } else
                                {
                                    localStream.ReadLine();
                                }
                                m++;
                            }

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine("||||||| " + dnode.InfoBase.FullName);


                            for (int p = 0; p < diff3Item.BaseAffectedLines; p++)
                            {
                                if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.RevertToBase
                                    || diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(baseStream.ReadLine());
                                } else
                                {
                                    baseStream.ReadLine();
                                }
                                o++;
                            }

                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine("=======");


                            for (int p = 0; p < diff3Item.RemoteAffectedLines; p++)
                            {
                                if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.ApplyRemote
                                    || diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(remoteStream.ReadLine());
                                } else
                                {
                                    remoteStream.ReadLine();
                                }
                                n++;
                            }


                            if (diff3Item.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine(">>>>>>> " + dnode.InfoRemote.FullName);

                            break;
                    }
                }

                // print end
                for (; o < diff.FilesLineCount.Base; o++) { writer.WriteLine(baseStream.ReadLine()); }
                //for (; m < dnode.Diff.FilesLineCount.Local; m++) { localStream.ReadLine(); }
                //for (; n < dnode.Diff.FilesLineCount.Remote; n++) { remoteStream.ReadLine(); }
            }

            // copy temporary file to correct location
            if (!isTemporary) return;

            File.Delete(CreatePath(node));
            File.Move(temporaryPath, CreatePath(node));
        }

        private static void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string CreatePath(INodeFileNode node)
        {
            string output = OutputFolder == null
                ? node.GetAbsolutePath(LocationEnum.OnBase)
                : Path.Combine(OutputFolder, node.Info.Name);

            CheckAndCreateDirectory(Path.GetDirectoryName(output));

            return output;
        }
    }
}
