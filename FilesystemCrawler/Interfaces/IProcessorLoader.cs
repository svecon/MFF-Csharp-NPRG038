using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FilesystemCrawler.Enums;

namespace FilesystemCrawler.Interfaces
{
    public interface IProcessorLoader
    {
        void LoadAll();

        IEnumerator<IProcessor> GetPreProcessors();

        IEnumerator<IProcessor> GetProcessors();

        IEnumerator<IProcessor> GetPostProcessors();

        void AddPreProcessor(IProcessor processor);

        void AddProcessor(IProcessor processor);

        void AddPostProcessor(IProcessor processor);
    }
}
