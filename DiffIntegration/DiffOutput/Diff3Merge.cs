using System;
using System.IO;
using System.Text;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Processors;
using DiffAlgorithm;
using DiffIntegration.DiffFilesystemTree;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Processor for merging 3-way diffed files.
    /// </summary>
    public class Diff3Merge : ProcessorAbstract
    {
        public override int Priority { get { return 125040; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.ThreeWay; } }

        public override void Process(IFilesystemTreeDirNode node)
        {
            // empty
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            if (dnode.Diff3 == null || dnode.Diff3.Items == null)
                return;

            var sb = new StringBuilder();

            using (StreamReader streamL = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader streamR = ((FileInfo)dnode.InfoRemote).OpenText())
            using (StreamReader streamO = ((FileInfo)dnode.InfoBase).OpenText())
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diff in dnode.Diff3.Items)
                {
                    // print between diffs
                    for (; o < diff.OldLineStart; o++) { Console.WriteLine(streamO.ReadLine()); }
                    for (; m < diff.NewLineStart; m++) { streamL.ReadLine(); }
                    for (; n < diff.HisLineStart; n++) { streamR.ReadLine(); }

                    // print diffs
                    switch (diff.Differeces)
                    {
                        case DifferencesStatusEnum.BaseLocalSame:
                            for (int p = 0; p < diff.NewAffectedLines; p++) { streamL.ReadLine(); m++; }
                            for (int p = 0; p < diff.OldAffectedLines; p++) { streamO.ReadLine(); o++; }
                            for (int p = 0; p < diff.HisAffectedLines; p++) { sb.AppendLine(streamR.ReadLine()); n++; }
                            break;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            for (int p = 0; p < diff.NewAffectedLines; p++) { sb.AppendLine(streamL.ReadLine()); m++; }
                            for (int p = 0; p < diff.OldAffectedLines; p++) { streamO.ReadLine(); o++; }
                            for (int p = 0; p < diff.HisAffectedLines; p++) { streamR.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            for (int p = 0; p < diff.NewAffectedLines; p++) { sb.AppendLine(streamL.ReadLine()); m++; }
                            for (int p = 0; p < diff.OldAffectedLines; p++) { streamO.ReadLine(); o++; }
                            for (int p = 0; p < diff.HisAffectedLines; p++) { streamR.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.AllDifferent:
                            sb.AppendLine("<<<<<<< " + dnode.InfoLocal.Name);
                            for (int p = 0; p < diff.NewAffectedLines; p++) { sb.AppendLine(streamL.ReadLine()); m++; }
                            sb.AppendLine("||||||| " + dnode.InfoBase.Name);
                            for (int p = 0; p < diff.OldAffectedLines; p++) { sb.AppendLine(streamO.ReadLine()); o++; }
                            sb.AppendLine("=======");
                            for (int p = 0; p < diff.HisAffectedLines; p++) { sb.AppendLine(streamR.ReadLine()); n++; }
                            sb.AppendLine(">>>>>>> " + dnode.InfoRemote.Name);
                            break;
                    }

                    Console.Write(sb.ToString());
                    sb.Clear();
                }

                // print end
                for (; o < dnode.Diff3.FilesLineCount.Old; o++) { Console.WriteLine(streamO.ReadLine()); }
                for (; m < dnode.Diff3.FilesLineCount.New; m++) { streamL.ReadLine(); }
                for (; n < dnode.Diff3.FilesLineCount.His; n++) { streamR.ReadLine(); }
            }
        }
    }
}
