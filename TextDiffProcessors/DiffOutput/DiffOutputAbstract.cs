using System.Collections.Generic;
using System.IO;

namespace DiffIntegration.DiffOutput
{
    /// <summary>
    /// Base implementation for 2-way diff outputs.
    /// </summary>
    /// <typeparam name="TU">Diff type</typeparam>
    /// <typeparam name="TV">DiffItem type</typeparam>
    public abstract class DiffOutputAbstract<TU, TV> : IDiffOutput<TV>
    {
        protected readonly FileInfo InfoLocal;
        protected readonly FileInfo InfoRemote;
        protected readonly TU Diff;

        private bool diffHasEnded;

        public bool DiffHasEnded
        {
            get
            {
                bool temp = diffHasEnded;
                diffHasEnded = false;
                return temp;
            }
            protected set { diffHasEnded = value; }
        }

        public TV CurrentDiffItem { get; protected set; }

        protected DiffOutputAbstract(FileInfo infoLocal, FileInfo infoRemote, TU diff)
        {
            InfoLocal = infoLocal;
            InfoRemote = infoRemote;
            Diff = diff;
        }

        public abstract IEnumerable<string> Print();
    }
}
