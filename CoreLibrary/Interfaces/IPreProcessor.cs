using System;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Standard PreProcessor interface.
    /// 
    /// All PreProcessors are run first.
    /// 
    /// They should be used to get more information about the node.
    /// </summary>
    public interface IPreProcessor : IProcessorBase
    {

    }
}
