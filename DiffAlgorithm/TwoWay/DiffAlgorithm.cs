using System;
using System.Collections.Generic;

namespace DiffAlgorithm.TwoWay
{
    /// <summary>
    /// This Class implements the Difference Algorithm published in
    /// "An O(ND) Difference Algorithm and its Variations" by Eugene Myers
    /// Algorithmica Vol. 1 No. 2, 1986, p 251.  
    /// 
    /// There are many C, Java, Lisp implementations public available but they all seem to come
    /// from the same source (diffutils) that is under the (unfree) GNU public License
    /// and cannot be reused as a sourcecode for a commercial application.
    /// There are very old C implementations that use other (worse) algorithms.
    /// Microsoft also published sourcecode of a diff-tool (windiff) that uses some tree data.
    /// Also, a direct transfer from a C source to C# is not easy because there is a lot of pointer
    /// arithmetic in the typical C solutions and i need a managed solution.
    /// These are the reasons why I implemented the original published algorithm from the scratch and
    /// make it avaliable without the GNU license limitations.
    /// I do not need a high performance diff tool because it is used only sometimes.
    /// I will do some performace tweaking when needed.
    /// 
    /// The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
    /// each line is converted into a (hash) number. See DiffText(). 
    /// 
    /// Some chages to the original algorithm:
    /// The original algorithm was described using a recursive approach and comparing zero indexed arrays.
    /// Extracting sub-arrays and rejoining them is very performance and memory intensive so the same
    /// (readonly) data arrays are passed arround together with their lower and upper bounds.
    /// This circumstance makes the LCS and SMS functions more complicate.
    /// I added some code to the LCS function to get a fast response on sub-arrays that are identical,
    /// completely deleted or inserted.
    /// 
    /// The result from a comparisation is stored in 2 arrays that flag for modified (deleted or inserted)
    /// lines in the 2 data arrays. These bits are then analysed to produce a array of Item objects.
    /// 
    /// Further possible optimizations:
    /// (first rule: don't do it; second: don't do it yet)
    /// The arrays DataA and DataB are passed as parameters, but are never changed after the creation
    /// so they can be members of the class to avoid the paramter overhead.
    /// In SMS is a lot of boundary arithmetic in the for-D and for-k loops that can be done by increment
    /// and decrement of local variables.
    /// The DownVector and UpVector arrays are alywas created and destroyed each time the SMS gets called.
    /// It is possible to reuse tehm when transfering them to members of the class.
    /// See TODO: hints.
    /// 
    /// diff.cs: A port of the algorythm to C#
    /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
    /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
    /// 
    /// Changes:
    /// 2002.09.20 There was a "hang" in some situations.
    /// Now I undestand a little bit more of the SMS algorithm. 
    /// There have been overlapping boxes; that where analyzed partial differently.
    /// One return-point is enough.
    /// A assertion was added in RunAndCreateDiffs when in debug-mode, that counts the number of equal (no modified) lines in both arrays.
    /// They must be identical.
    /// 
    /// 2003.02.07 Out of bounds error in the Up/Down vector arrays in some situations.
    /// The two vetors are now accessed using different offsets that are adjusted using the start k-Line. 
    /// A test case is added. 
    /// 
    /// 2006.03.05 Some documentation and a direct Diff entry point.
    /// 
    /// 2006.03.08 Refactored the API to static methods on the Diff class to make usage simpler.
    /// 2006.03.10 using the standard Debug class for self-test now.
    ///            compile with: csc /target:exe /out:diffTest.exe /d:DEBUG /d:TRACE /d:SELFTEST Diff.cs
    /// 2007.01.06 license agreement changed to a BSD style license.
    /// 2007.06.03 added the Optimize method.
    /// 2007.09.23 UpVector and DownVector optimization by Jan Stoklasa ().
    /// 2008.05.31 Adjusted the testing code that failed because of the Optimize method (not a bug in the diff algorithm).
    /// 2008.10.08 Fixing a test case and adding a new test case.
    /// </summary>
    class DiffAlgorithm
    {
        private readonly DiffData dataA;
        private readonly DiffData dataB;

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

        /// <summary>
        /// Short Middle Snake point
        /// 
        /// Strucure to hold a SMS point in the Diff algorithm.
        /// </summary>
        internal struct SmsPoint
        {
            internal int X, Y;

            public SmsPoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public DiffAlgorithm(DiffData dataA, DiffData dataB)
        {
            this.dataA = dataA;
            this.dataB = dataB;
        }

        /// <summary>
        /// Longest Common Subsequence
        /// 
        /// This is the divide-and-conquer implementation of the longes common-subsequence (LCS) 
        /// algorithm.
        /// The published algorithm passes recursively parts of the A and B sequences.
        /// To avoid copying these arrays the lower and upper bounds are passed while the sequences stay constant.
        /// 
        /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
        /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
        /// </summary>
        /// <param name="lowerA">Lower bound for data A</param>
        /// <param name="upperA">Upper bound for data A</param>
        /// <param name="lowerB">Lower bound for data A</param>
        /// <param name="upperB">Upper bound for data B</param>
        /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
        /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
        private void Lcs(int lowerA, int upperA, int lowerB, int upperB, int[] downVector, int[] upVector)
        {
            // Fast walkthrough equal lines at the start
            while (lowerA < upperA && lowerB < upperB && dataA.Data[lowerA] == dataB.Data[lowerB])
            {
                lowerA++; lowerB++;
            }

            // Fast walkthrough equal lines at the end
            while (lowerA < upperA && lowerB < upperB && dataA.Data[upperA - 1] == dataB.Data[upperB - 1])
            {
                --upperA; --upperB;
            }

            if (lowerA == upperA)
            {
                // mark as inserted lines.
                while (lowerB < upperB)
                    dataB.Modified[lowerB++] = true;

            } else if (lowerB == upperB)
            {
                // mark as deleted lines.
                while (lowerA < upperA)
                    dataA.Modified[lowerA++] = true;

            } else
            {
                // Find the middle snakea and length of an optimal path for A and B
                SmsPoint snake = Sms(lowerA, upperA, lowerB, upperB, downVector, upVector);

                // The path is from LowerX to (x,y) and (x,y) ot UpperX
                Lcs(lowerA, snake.X, lowerB, snake.Y, downVector, upVector);
                Lcs(snake.X, upperA, snake.Y, upperB, downVector, upVector);
            }
        }

        /// <summary>
        /// This is the algorithm to find the Shortest Middle Snake (SMS).
        ///
        /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
        /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
        /// </summary>
        /// <param name="lowerA">Lower bound for data A.</param>
        /// <param name="upperA">Upper bound for data A.</param>
        /// <param name="lowerB">Lower bound for data B.</param>
        /// <param name="upperB">Upper bound for data B.</param>
        /// <param name="downVector">a vector for the (0,0) to (x,y) search. Passed as a parameter for speed reasons.</param>
        /// <param name="upVector">a vector for the (u,v) to (N,M) search. Passed as a parameter for speed reasons.</param>
        /// <returns></returns>
        private SmsPoint Sms(int lowerA, int upperA, int lowerB, int upperB, int[] downVector, int[] upVector)
        {
            int max = dataA.Length + dataB.Length + 1;

            int downK = lowerA - lowerB; // the k-line to start the forward search
            int upK = upperA - upperB; // the k-line to start the reverse search

            int delta = (upperA - lowerA) - (upperB - lowerB);
            bool oddDelta = (delta & 1) != 0;

            // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
            // and are access using diffItemsList specific offset: UpOffset UpVector and DownOffset for DownVektor
            int downOffset = max - downK;
            int upOffset = max - upK;

            int maxD = ((upperA - lowerA + upperB - lowerB) / 2) + 1;

            // init vectors
            downVector[downOffset + downK + 1] = lowerA;
            upVector[upOffset + upK - 1] = upperA;

            for (int d = 0; d <= maxD; d++)
            {

                // Extend the forward path.
                for (int k = downK - d; k <= downK + d; k += 2)
                {
                    // find the only or better starting point
                    int x;
                    if (k == downK - d)
                    {
                        x = downVector[downOffset + k + 1]; // down
                    } else
                    {
                        x = downVector[downOffset + k - 1] + 1; // diffItemsList step to the right
                        if ((k < downK + d) && (downVector[downOffset + k + 1] >= x))
                            x = downVector[downOffset + k + 1]; // down
                    }
                    int y = x - k;

                    // find the end of the furthest reaching forward D-path in diagonal k.
                    while ((x < upperA) && (y < upperB) && (dataA.Data[x] == dataB.Data[y]))
                    {
                        x++; y++;
                    }
                    downVector[downOffset + k] = x;

                    // overlap ?
                    if (!oddDelta || (upK - d >= k) || (k >= upK + d)) continue;
                    if (upVector[upOffset + k] > downVector[downOffset + k]) continue;

                    return new SmsPoint(downVector[downOffset + k], downVector[downOffset + k] - k);
                }

                // Extend the reverse path.
                for (int k = upK - d; k <= upK + d; k += 2)
                {
                    // find the only or better starting point
                    int x, y;
                    if (k == upK + d)
                    {
                        x = upVector[upOffset + k - 1]; // up
                    } else
                    {
                        x = upVector[upOffset + k + 1] - 1; // left
                        if ((k > upK - d) && (upVector[upOffset + k - 1] < x))
                            x = upVector[upOffset + k - 1]; // up
                    }
                    y = x - k;

                    while ((x > lowerA) && (y > lowerB) && (dataA.Data[x - 1] == dataB.Data[y - 1]))
                    {
                        x--; y--; // diagonal
                    }
                    upVector[upOffset + k] = x;

                    // overlap ?
                    if (oddDelta || (downK - d > k) || (k > downK + d)) continue;
                    if (upVector[upOffset + k] > downVector[downOffset + k]) continue;

                    return new SmsPoint(downVector[downOffset + k], downVector[downOffset + k] - k);
                }

            }

            throw new ApplicationException("The algorithm should never come here.");
        }

        /// <summary>
        /// Run the algorithm.
        /// </summary>
        /// <param name="forceClean">Cleans diff bool[] to allow reusability.</param>
        public void Run(bool forceClean = false)
        {
            // needed when dataA is shared for two instances in Diff3 algorithm
            if (forceClean)
                dataA.RecreateModified();

            int max = dataA.Length + dataB.Length + 1;
            // vector for the (0,0) to (x,y) search
            var downVector = new int[2 * max + 2];
            // vector for the (u,v) to (N,M) search
            var upVector = new int[2 * max + 2];

            Lcs(0, dataA.Length, 0, dataB.Length, downVector, upVector);

            Optimize(dataA);
            Optimize(dataB);
        }

        /// <summary>
        /// Run the algorithm, create and return 2-way diffs.
        /// </summary>
        /// <param name="forceClean">Cleans diff bool[] to allow reusability.</param>
        /// <returns>DiffItems array.</returns>
        public DiffItem[] RunAndCreateDiffs(bool forceClean = false)
        {
            Run(forceClean);

            return CreateDiffs();
        }

        /// <summary>
        /// Scan the tables of which lines are inserted and deleted,
        /// producing an edit script in forward order.
        /// 
        /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
        /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
        /// </summary>
        /// <returns>DiffItems array.</returns>
        protected DiffItem[] CreateDiffs()
        {
            var diffItemsList = new List<DiffItem>();

            int lineA = 0;
            int lineB = 0;
            while (lineA < dataA.Length || lineB < dataB.Length)
            {
                if ((lineA < dataA.Length) && (!dataA.Modified[lineA])
                  && (lineB < dataB.Length) && (!dataB.Modified[lineB]))
                {
                    // equal lines
                    lineA++;
                    lineB++;

                } else
                {
                    // maybe deleted and/or inserted lines
                    int startA = lineA;
                    int startB = lineB;

                    while (lineA < dataA.Length && (lineB >= dataB.Length || dataA.Modified[lineA]))
                        lineA++;

                    while (lineB < dataB.Length && (lineA >= dataA.Length || dataB.Modified[lineB]))
                        lineB++;

                    if ((startA < lineA) || (startB < lineB))
                    {
                        // store diffItemsList new difference-item
                        diffItemsList.Add(new DiffItem(startA, startB, lineA - startA, lineB - startB));
                    }
                }
            }

            return diffItemsList.ToArray();
        }

        /// <summary>
        /// If a sequence of modified lines starts with a line that contains the same content
        /// as the line that appends the changes, the difference sequence is modified so that the
        /// appended line and not the starting line is marked as modified.
        /// This leads to more readable diff sequences when comparing text files.
        /// 
        /// Copyright (c) by Matthias Hertel, http://www.mathertel.de
        /// This work is licensed under a BSD style license. See http://www.mathertel.de/License.aspx
        /// </summary>
        /// <param name="data">A Diff data buffer containing the identified changes.</param>
        private static void Optimize(DiffData data)
        {
            int startPos = 0;
            while (startPos < data.Length)
            {
                while ((startPos < data.Length) && (data.Modified[startPos] == false))
                    startPos++;
                
                int endPos = startPos;
                while ((endPos < data.Length) && data.Modified[endPos])
                    endPos++;

                if ((endPos < data.Length) && (data.Data[startPos] == data.Data[endPos]))
                {
                    data.Modified[startPos] = false;
                    data.Modified[endPos] = true;
                } else
                {
                    startPos = endPos;
                }
            }
        }
    }
}
