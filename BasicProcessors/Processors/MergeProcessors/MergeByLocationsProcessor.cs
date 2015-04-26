using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings.Attributes;
using DiffIntegration.DiffFilesystemTree;

namespace BasicProcessors.Processors.MergeProcessors
{
    [Processor(ProcessorTypeEnum.Merge, 10, DiffModeEnum.ThreeWay)]
    public class MergeByLocationsProcessor : ProcessorAbstract
    {
        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var diffNode = node as DiffFileNode;

            if (diffNode == null)
                return;

            if (diffNode.Action != PreferedActionThreeWayEnum.Default)
                diffNode.Status = NodeStatusEnum.WasMerged;

            switch (diffNode.Action)
            {
                case PreferedActionThreeWayEnum.ApplyLocal:
                    ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    return;
                case PreferedActionThreeWayEnum.ApplyRemote:
                    ((FileInfo)node.InfoRemote).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    return;
                case PreferedActionThreeWayEnum.RevertToBase:
                    ((FileInfo)node.InfoBase).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    return;
            }

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                    File.Delete(CreatePath(node));
                    node.RemoveInfoFromLocation(LocationEnum.OnBase);
                    return; // delete

                case LocationCombinationsEnum.OnLocal:
                    // one new file
                    ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    break;
                case LocationCombinationsEnum.OnRemote:
                    // one new file
                    ((FileInfo)node.InfoRemote).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    break;
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                    {
                        File.Delete(CreatePath(node));
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnBaseRemote:

                    if (node.Differences == DifferencesStatusEnum.BaseRemoteSame)
                    {
                        File.Delete(CreatePath(node));
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnLocalRemote:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                        node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                        return; // copy
                    }
                    break;

                case LocationCombinationsEnum.OnAll3:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                        node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                        return; // copy
                    }
                    break;
            }
        }

        private void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string CreatePath(IFilesystemTreeFileNode node)
        {
            string output = OutputFolder == null
                ? node.GetAbsolutePath(LocationEnum.OnBase)
                : Path.Combine(OutputFolder, node.Info.Name);

            CheckAndCreateDirectory(Path.GetDirectoryName(output));

            return output;
        }

    }
}
