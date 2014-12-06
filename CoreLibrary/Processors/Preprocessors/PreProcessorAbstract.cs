using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors.Preprocessors
{
    /// <summary>
    /// AbstractPreProcessor is a class that all PreProcessors should inherit from.
    /// 
    /// Contain some helper methods for given processor.
    /// </summary>
    public abstract class PreProcessorAbstract : BaseProcessorAbstract, IPreProcessor
    {

    }
}
