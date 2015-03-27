using System;
using System.IO;
using System.Linq;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Postprocessors;
using CoreLibrary.Settings.Attributes;
using DiffAlgorithm;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.Processors.Postprocessors
{
    /// <summary>
    /// Processor for merging 3-way diffed files.
    /// </summary>
    public class MergeThreeWayProcessor : PostProcessorAbstract
    {
        public override int Priority { get { return 301; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }

        [Settings("Merge folders and files.", "merge", "m")]
        public bool IsEnabled = false;

        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        public enum DefaultActionEnum
        {
            WriteConflicts, RevertToBase, ApplyLocal, ApplyRemote
        }

        [Settings("Default action for merging files.", "3merge-default", "3d")]
        public DefaultActionEnum DefaultAction;

        public override void Process(IFilesystemTreeDirNode node)
        {
            // if OutputFolder is unset
            if (IsEnabled && OutputFolder == null)
            {
                // set it to root folder of InfoBase
                OutputFolder = node.InfoBase.FullName;
            }
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

            // this means that the files are not in a folder
            if (OutputFolder == null)
                OutputFolder = Path.GetDirectoryName(node.InfoBase.FullName);

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                    return; // delete
                case LocationCombinationsEnum.OnLocal:
                case LocationCombinationsEnum.OnRemote:
                    // one new file
                    ((FileInfo)node.Info).CopyTo(CreatePath(node), true);
                    break;
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                        return; // delete

                    node.Status = NodeStatusEnum.IsConflicting;

                    break;
                case LocationCombinationsEnum.OnBaseRemote:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                        return; // delete

                    node.Status = NodeStatusEnum.IsConflicting;

                    break;
                case LocationCombinationsEnum.OnLocalRemote:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                    {
                        ((FileInfo)node.Info).CopyTo(CreatePath(node), true);
                        return; // copy
                    }

                    node.Status = NodeStatusEnum.IsConflicting;

                    break;
            }


            // only continue if all 3 files are present
            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnAll3)
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

                foreach (Diff3Item diff in dnode.Diff3.Items)
                {
                    // change default action depending on processor settings
                    if (diff.Action == Diff3ItemActionEnum.Default)
                    {
                        switch (DefaultAction)
                        {
                            case DefaultActionEnum.WriteConflicts:
                                // keep default
                                break;
                            case DefaultActionEnum.RevertToBase:
                                diff.Action = Diff3ItemActionEnum.RevertToBase;
                                break;
                            case DefaultActionEnum.ApplyLocal:
                                diff.Action = Diff3ItemActionEnum.ApplyLocal;
                                break;
                            case DefaultActionEnum.ApplyRemote:
                                diff.Action = Diff3ItemActionEnum.ApplyRemote;
                                break;
                        }
                    }

                    // print between diffs
                    for (; o < diff.BaseLineStart; o++) { writer.WriteLine(baseStream.ReadLine()); }
                    for (; m < diff.LocalLineStart; m++) { localStream.ReadLine(); }
                    for (; n < diff.RemoteLineStart; n++) { remoteStream.ReadLine(); }

                    // if there is an action asociated:
                    if (diff.Action != Diff3ItemActionEnum.Default)
                    {
                        for (int p = 0; p < diff.LocalAffectedLines; p++)
                        {
                            m++;

                            if (diff.Action == Diff3ItemActionEnum.ApplyLocal)
                                writer.WriteLine(localStream.ReadLine());
                            else
                                localStream.ReadLine();
                        }
                        for (int p = 0; p < diff.BaseAffectedLines; p++)
                        {
                            o++;

                            if (diff.Action == Diff3ItemActionEnum.RevertToBase)
                                writer.WriteLine(baseStream.ReadLine());
                            else
                                baseStream.ReadLine();

                        }
                        for (int p = 0; p < diff.RemoteAffectedLines; p++)
                        {
                            n++;

                            if (diff.Action == Diff3ItemActionEnum.ApplyRemote)
                                writer.WriteLine(remoteStream.ReadLine());
                            else
                                remoteStream.ReadLine();
                        }

                        continue;
                    }

                    // print diffs
                    switch (diff.Differeces)
                    {
                        case DifferencesStatusEnum.BaseLocalSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { localStream.ReadLine(); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { writer.WriteLine(remoteStream.ReadLine()); n++; }
                            break;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.AllDifferent:

                            if (diff.Action == Diff3ItemActionEnum.Default)
                            {
                                node.Status = NodeStatusEnum.HasConflicts;
                                writer.WriteLine("<<<<<<< " + dnode.InfoLocal.FullName);
                            }


                            for (int p = 0; p < diff.LocalAffectedLines; p++)
                            {
                                if (diff.Action == Diff3ItemActionEnum.ApplyLocal
                                    || diff.Action == Diff3ItemActionEnum.Default)
                                {
                                    writer.WriteLine(localStream.ReadLine());
                                } else
                                {
                                    localStream.ReadLine();
                                }
                                m++;
                            }

                            if (diff.Action == Diff3ItemActionEnum.Default)
                                writer.WriteLine("||||||| " + dnode.InfoBase.FullName);


                            for (int p = 0; p < diff.BaseAffectedLines; p++)
                            {
                                if (diff.Action == Diff3ItemActionEnum.RevertToBase
                                    || diff.Action == Diff3ItemActionEnum.Default)
                                {
                                    writer.WriteLine(baseStream.ReadLine());
                                } else
                                {
                                    baseStream.ReadLine();
                                }
                                o++;
                            }

                            if (diff.Action == Diff3ItemActionEnum.Default)
                                writer.WriteLine("=======");


                            for (int p = 0; p < diff.RemoteAffectedLines; p++)
                            {
                                if (diff.Action == Diff3ItemActionEnum.ApplyRemote
                                    || diff.Action == Diff3ItemActionEnum.Default)
                                {
                                    writer.WriteLine(remoteStream.ReadLine());
                                } else
                                {
                                    remoteStream.ReadLine();
                                }
                                n++;
                            }


                            if (diff.Action == Diff3ItemActionEnum.Default)
                                writer.WriteLine(">>>>>>> " + dnode.InfoRemote.FullName);

                            break;
                    }
                }

                // print end
                for (; o < dnode.Diff3.FilesLineCount.Base; o++) { writer.WriteLine(baseStream.ReadLine()); }
                //for (; m < dnode.Diff3.FilesLineCount.Local; m++) { localStream.ReadLine(); }
                //for (; n < dnode.Diff3.FilesLineCount.Remote; n++) { remoteStream.ReadLine(); }
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
