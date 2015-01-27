using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

namespace CoreLibrary.Processors.Postprocessors
{
    /// <summary>
    /// AbstractPostProcessor is a class that all PostProcessors should inherit from.
    /// 
    /// Contain some helper methods for given processor.
    /// </summary>
    public abstract class PostProcessorAbstract : BaseProcessorAbstract, IPostProcessor
    {
        protected override bool checkStatus(IFilesystemTreeAbstractNode node)
        {
            if (!base.checkStatus(node))
                return false;

            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            return true;
        }
    }
}
