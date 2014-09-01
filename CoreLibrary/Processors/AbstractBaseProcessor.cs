using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLibrary.Interfaces;
using CoreLibrary.Enums;
using CoreLibrary.Settings;

namespace CoreLibrary.Processors
{
    /// <summary>
    /// AbstractProcessor is a base class for all types of processors.
    /// 
    /// Contain some helper methods that are shared for all processors.
    /// </summary>
    public abstract class AbstractBaseProcessor : IProcessorBase
    {
        /// <summary>
        /// Mode is a mask for all possible DiffModeEnum values.
        /// </summary>
        public abstract DiffModeEnum Mode { get; }

        public abstract int Priority { get; }

        protected List<SettingsAbstract> settings;

        /// <summary>
        /// Checks whether the node still should be processed or not.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the node should be processed.</returns>
        protected virtual bool checkStatus(IFilesystemTreeAbstractNode node)
        {
            if (node.Status == NodeStatusEnum.HasError)
                return false;

            if (node.Status == NodeStatusEnum.IsIgnored)
                return false;

            return true;
        }

        /// <summary>
        /// Checks whether current Processor is compatible with node's mode.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the processor is compatible.</returns>
        protected virtual bool checkMode(IFilesystemTreeAbstractNode node)
        {
            if ((node.Mode & Mode) == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Check node's mode and status for compatibility.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the node should be processed.</returns>
        protected bool checkModeAndStatus(IFilesystemTreeAbstractNode node)
        {
            return checkMode(node) && checkStatus(node);
        }

        public IEnumerable<SettingsAbstract> GetSettings()
        {
            foreach (var s in settings)
            {
                yield return s;
            }
        }

        public abstract void Process(IFilesystemTreeDirNode node);

        public abstract void Process(IFilesystemTreeFileNode node);
    }
}
