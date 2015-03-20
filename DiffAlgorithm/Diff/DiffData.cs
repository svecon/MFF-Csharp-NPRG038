namespace DiffAlgorithm.Diff
{
    /// <summary>
    /// Container for holding which lines have been changed in a file.
    /// Used in the main diff algorithm.
    /// </summary>
    internal class DiffData
    {
        /// <summary>Number of elements (lines).</summary>
        internal int Length;

        /// <summary>Buffer of numbers that will be compared.</summary>
        internal int[] Data;

        /// <summary>
        /// Array of booleans that flag for modified data.
        /// This is the result of the diff.
        /// This means DeletedInOld in the first Data or inserted in the second Data.
        /// </summary>
        internal bool[] Modified;

        /// <summary>
        /// Initialize the Diff-Data buffer.
        /// </summary>
        /// <param name="initData">reference to the buffer</param>
        internal DiffData(int[] initData)
        {
            Data = initData;
            Length = initData.Length;
            RecreateModified();
        }

        public void RecreateModified()
        {
            Modified = new bool[Length + 2];
        }

    }
}
