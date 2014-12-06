﻿using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

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

            if (node.Status == NodeStatusEnum.WasDiffed)
                return false;

            if (node.Status == NodeStatusEnum.WasDiffedWaitingForMerge)
                return false;

            return true;
        }
    }
}