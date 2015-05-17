using System.Collections.Generic;
using System.IO;

namespace TextDiffProcessors.DiffOutput
{
    /// <summary>
    /// Base implementation for 2-way diff outputs.
    /// </summary>
    /// <typeparam name="TU">Diff type</typeparam>
    /// <typeparam name="TV">DiffItem type</typeparam>
    public abstract class DiffOutputAbstract<TU, TV> : IDiffOutput<TV>
    {
        /// <summary>
        /// Info for local file.
        /// </summary>
        protected readonly FileInfo InfoLocal;

        /// <summary>
        /// Info for remote file.
        /// </summary>
        protected readonly FileInfo InfoRemote;

        /// <summary>
        /// Calculated diff.
        /// </summary>
        protected readonly TU Diff;

        /// <summary>
        /// Field for <see cref="DiffHasEnded"/>
        /// </summary>
        private bool diffHasEnded;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public TV CurrentDiffItem { get; protected set; }

        /// <summary>
        /// Initializes new instance of the <see cref="DiffOutputAbstract{TU,TV}"/>
        /// </summary>
        /// <param name="infoLocal">Info for the local file.</param>
        /// <param name="infoRemote">Info for the remote file.</param>
        /// <param name="diff">Calculated diff.</param>
        protected DiffOutputAbstract(FileInfo infoLocal, FileInfo infoRemote, TU diff)
        {
            InfoLocal = infoLocal;
            InfoRemote = infoRemote;
            Diff = diff;
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> Print();
    }
}
