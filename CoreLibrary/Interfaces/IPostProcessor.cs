using System;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Standard PostProcessor interface.
    /// 
    /// All PostProcessors are run last. 
    /// 
    /// They should be used for manupulating with physical copies of the files
    /// (creating, moving, deleting, renaming).
    /// </summary>
    public interface IPostProcessor : IProcessorBase
    {

    }
}
