using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.MergeProcessors
{
    [Processor(ProcessorTypeEnum.Merge, 10, DiffModeEnum.ThreeWay)]
    public class MergeByLocationsProcessor : ProcessorAbstract
    {
        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        protected override void ProcessChecked(INodeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            switch (diffNode.Action)
            {
                case PreferedActionThreeWayEnum.ApplyLocal:
                    ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
                case PreferedActionThreeWayEnum.ApplyRemote:
                    ((FileInfo)node.InfoRemote).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
                case PreferedActionThreeWayEnum.RevertToBase:
                    ((FileInfo)node.InfoBase).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
            }

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                    File.Delete(CreatePath(node));
                    node.RemoveInfoFromLocation(LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return; // delete

                case LocationCombinationsEnum.OnLocal:
                    // one new file
                    ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    break;
                case LocationCombinationsEnum.OnRemote:
                    // one new file
                    ((FileInfo)node.InfoRemote).CopyTo(CreatePath(node), true);
                    node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    break;
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                    {
                        File.Delete(CreatePath(node));
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnBaseRemote:

                    if (node.Differences == DifferencesStatusEnum.BaseRemoteSame)
                    {
                        File.Delete(CreatePath(node));
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnLocalRemote:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                        node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // copy
                    }
                    break;

                case LocationCombinationsEnum.OnAll3:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(CreatePath(node), true);
                        node.AddInfoFromLocation(new FileInfo(CreatePath(node)), LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
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
