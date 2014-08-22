using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilesystemCrawler.Interfaces;

namespace FilesystemCrawler
{
    class ProcessorsLoader : IProcessorLoader
    {

        SortedList<int, IProcessor> PreProcessors;

        SortedList<int, IProcessor> Processors;

        SortedList<int, IProcessor> PostProcessors;

        public ProcessorsLoader()
        {
            PreProcessors = new SortedList<int, IProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IProcessor>();
        }

        public void LoadAll()
        {

        }

        public void AddPreProcessor(IProcessor processor)
        {
            PreProcessors.Add(processor.Priority, processor);
        }

        public void AddProcessor(IProcessor processor)
        {
            Processors.Add(processor.Priority, processor);
        }

        public void AddPostProcessor(IProcessor processor)
        {
            PostProcessors.Add(processor.Priority, processor);
        }


        public IEnumerator<IProcessor> GetPreProcessors()
        {
            foreach (var processor in PreProcessors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerator<IProcessor> GetProcessors()
        {
            foreach (var processor in Processors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerator<IProcessor> GetPostProcessors()
        {
            foreach (var processor in PostProcessors)
            {
                yield return processor.Value;
            }
        }
    }
}
