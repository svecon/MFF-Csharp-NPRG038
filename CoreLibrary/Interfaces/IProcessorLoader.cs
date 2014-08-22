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

        IEnumerable<IProcessor> GetPreProcessors();

        IEnumerable<IProcessor> GetProcessors();

        IEnumerable<IProcessor> GetPostProcessors();

        void AddPreProcessor(IProcessor processor);

        void AddProcessor(IProcessor processor);

        void AddPostProcessor(IProcessor processor);
    }
}
