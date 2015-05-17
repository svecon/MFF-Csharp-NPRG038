using System.IO;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;
using CoreLibrary.Helpers;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace BasicProcessors.MergeProcessors
{
    /// <summary>
    /// Processor that merges some trivial cases where not all files are present.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 10, DiffModeEnum.ThreeWay)]
    public class MergeByLocationsProcessor : ProcessorAbstract
    {
        /// <summary>
        /// Output folder of the merging.
        /// </summary>
        [Settings("Output for the resulting merge.", "output", "o")]
        public string Output;

        /// <inheritdoc />
        protected override void ProcessChecked(INodeDirNode node)
        {
        }

        /// <inheritdoc />
        protected override void ProcessChecked(INodeFileNode node)
        {
            var diffNode = node as FileDiffNode;

            if (diffNode == null)
                return;

            switch (diffNode.Action)
            {
                case PreferedActionThreeWayEnum.ApplyLocal:
                    ((FileInfo)node.InfoLocal).CopyTo(node.CreatePath(Output), true);
                    node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
                case PreferedActionThreeWayEnum.ApplyRemote:
                    ((FileInfo)node.InfoRemote).CopyTo(node.CreatePath(Output), true);
                    node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
                case PreferedActionThreeWayEnum.RevertToBase:
                    ((FileInfo)node.InfoBase).CopyTo(node.CreatePath(Output), true);
                    node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return;
            }

            switch ((LocationCombinationsEnum)node.Location)
            {
                case LocationCombinationsEnum.OnBase:
                    File.Delete(node.CreatePath(Output));
                    node.RemoveInfoFromLocation(LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    return; // delete

                case LocationCombinationsEnum.OnLocal:
                    // one new file
                    ((FileInfo)node.InfoLocal).CopyTo(node.CreatePath(Output), true);
                    node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    break;
                case LocationCombinationsEnum.OnRemote:
                    // one new file
                    ((FileInfo)node.InfoRemote).CopyTo(node.CreatePath(Output), true);
                    node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                    diffNode.Status = NodeStatusEnum.WasMerged;
                    break;
                case LocationCombinationsEnum.OnBaseLocal:

                    if (node.Differences == DifferencesStatusEnum.BaseLocalSame)
                    {
                        File.Delete(((FileInfo)node.InfoBase).FullName);
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnBaseRemote:

                    if (node.Differences == DifferencesStatusEnum.BaseRemoteSame)
                    {
                        File.Delete(((FileInfo)node.InfoBase).FullName);
                        node.RemoveInfoFromLocation(LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // delete
                    }

                    break;
                case LocationCombinationsEnum.OnLocalRemote:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(node.CreatePath(Output), true);
                        node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // copy
                    }
                    break;

                case LocationCombinationsEnum.OnAll3:

                    if (node.Differences == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        ((FileInfo)node.InfoLocal).CopyTo(node.CreatePath(Output), true);
                        node.AddInfoFromLocation(new FileInfo(node.CreatePath(Output)), LocationEnum.OnBase);
                        diffNode.Status = NodeStatusEnum.WasMerged;
                        return; // copy
                    }
                    break;
            }
        }
    }
}
