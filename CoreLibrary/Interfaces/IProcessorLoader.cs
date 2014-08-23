using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreLibrary.Enums;

namespace CoreLibrary.Interfaces
{
    public interface IProcessorLoader
    {
        void Load();

        IEnumerable<IPreProcessor> GetPreProcessors();

        IEnumerable<IProcessor> GetProcessors();

        IEnumerable<IPostProcessor> GetPostProcessors();

        void AddProcessor(IPreProcessor processor);

        void AddProcessor(IProcessor processor);

        void AddProcessor(IPostProcessor processor);
    }
}
