using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors.Postprocessors
{
    /// <summary>
    /// AbstractPostProcessor is a class that all PostProcessors should inherit from.
    /// 
    /// Contain some helper methods for given processor.
    /// </summary>
    public abstract class PostProcessorAbstract : BaseProcessorAbstract, IPostProcessor
    {
        protected override bool CheckStatus(IFilesystemTreeAbstractNode node)
        {
            if (!base.CheckStatus(node))
                return false;

            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            return true;
        }
    }
}
