﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffAlgorithm
{
    internal class DiffData
    {

        /// <summary>Number of elements (lines).</summary>
        internal int Length;

        /// <summary>Buffer of numbers that will be compared.</summary>
        internal int[] Data;

        /// <summary>
        /// Array of booleans that flag for modified data.
        /// This is the result of the diff.
        /// This means DeletedInA in the first Data or inserted in the second Data.
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
            Modified = new bool[Length + 2];
        }

    }
}
