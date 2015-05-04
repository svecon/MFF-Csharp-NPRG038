
using System.Collections.Generic;

namespace TextDiffProcessors.DiffOutput
{
    /// <summary>
    /// Interface for output differs.
    /// </summary>
    interface IDiffOutput<out T>
    {
        /// <summary>
        /// Main method which iterates over all lines to be printed by the diff.
        /// </summary>
        /// <returns>Diff line by line</returns>
        IEnumerable<string> Print();

        /// <summary>
        /// True when the one diff chunk ended iterating and has been all printed out.
        /// </summary>
        bool DiffHasEnded { get; }

        /// <summary>
        /// Currently printing diff chunk.
        /// </summary>
        T CurrentDiffItem { get; }
    }
}
