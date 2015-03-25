using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Postprocessors;
using CoreLibrary.Settings.Attributes;

namespace DiffIntegration.Processors.Postprocessors
{
    /// <summary>
    /// Processor that writes all nodes into a single file.
    /// 
    /// Only written out of curiosity about how much lines does this project contain.
    /// This serves a good demonstration about how powerful processors are.
    /// </summary>
    public class OutputSingleFileProcessor : PostProcessorAbstract
    {
        public override int Priority { get { return 1000; } }

        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay; } }

        [Settings("Output all files into a one single file.", "single-file")]
        public string OutputPath;

        public override void Process(IFilesystemTreeDirNode node)
        {

        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (OutputPath == null)
                return;

            if (node.FileType != FileTypeEnum.Text)
                return;

            using (StreamReader reader = ((FileInfo)node.Info).OpenText())
            using (StreamWriter writer = File.AppendText(OutputPath))
                while (!reader.EndOfStream)
                    writer.WriteLine(reader.ReadLine());
        }

    }
}
