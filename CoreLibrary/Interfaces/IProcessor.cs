using System;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Standard processor interface.
    /// 
    /// Processors are run after all PreProcessors are finished.
    /// 
    /// Processors should get more detailed information about the file.
    /// The information from the processors may play a huge role in PostProcessing phase.
    /// </summary>
    public interface IProcessor : IProcessorBase
    {

    }
}
