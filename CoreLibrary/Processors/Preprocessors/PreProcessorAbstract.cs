using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;

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
