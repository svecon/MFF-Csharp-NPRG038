using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors.Processors
{
    /// <summary>
    /// ProcessorAbstract is a class that all Processors should inherit from.
    /// 
    /// Contain some helper methods for given processor.
    /// </summary>
    public abstract class ProcessorAbstract : BaseProcessorAbstract, IProcessor
    {
        protected override bool CheckStatus(IFilesystemTreeAbstractNode node)
        {
            if (!base.CheckStatus(node))
                return false;

            //switch (node.Status)
            //{
            //    case NodeStatusEnum.WasDiffed:
            //        return false;
            //}

            return true;
        }
    }
}
