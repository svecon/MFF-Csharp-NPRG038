using System;
using System.Collections.Generic;

namespace DiffAlgorithm
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
    /// Created by Matthias Hertel, see http://www.mathertel.de
    /// This work is licensed under a Creative Commons Attribution 2.0 Germany License.
    /// see http://creativecommons.org/licenses/by/2.0/de/
    /// 
    /// Changes:
    /// 2002.09.20 There was a "hang" in some situations.
    /// Now I undestand a little bit more of the SMS algorithm. 
    /// There have been overlapping boxes; that where analyzed partial differently.
    /// One return-point is enough.
    /// A assertion was added in CreateDiffs when in debug-mode, that counts the number of equal (no modified) lines in both arrays.
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
    /// </summary>
    class DiffAlgorithm
    {
        private readonly DiffData dataA;
        private readonly DiffData dataB;

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
        /// Created by Matthias Hertel, see http://www.mathertel.de
        /// This work is licensed under a Creative Commons Attribution 2.0 Germany License.
        /// see http://creativecommons.org/licenses/by/2.0/de/
        /// </summary>
        /// <param name="lowerA">Lower bound for data A</param>
        /// <param name="upperA">Upper bound for data A</param>
        /// <param name="lowerB">Lower bound for data A</param>
        /// <param name="upperB">Upper bound for data B</param>
        private void Lcs(int lowerA, int upperA, int lowerB, int upperB)
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
                SmsPoint snake = Sms(lowerA, upperA, lowerB, upperB);

                // The path is from LowerX to (x,y) and (x,y) ot UpperX
                Lcs(lowerA, snake.X, lowerB, snake.Y);
                Lcs(snake.X, upperA, snake.Y, upperB);
            }
        }

        /// <summary>
        /// This is the algorithm to find the Shortest Middle Snake (SMS).
        ///
        /// Created by Matthias Hertel, see http://www.mathertel.de
        /// This work is licensed under a Creative Commons Attribution 2.0 Germany License.
        /// see http://creativecommons.org/licenses/by/2.0/de/
        /// </summary>
        /// <param name="lowerA">Lower bound for data A.</param>
        /// <param name="upperA">Upper bound for data A.</param>
        /// <param name="lowerB">Lower bound for data B.</param>
        /// <param name="upperB">Upper bound for data B.</param>
        /// <returns></returns>
        private SmsPoint Sms(int lowerA, int upperA, int lowerB, int upperB)
        {
            int max = dataA.Length + dataB.Length + 1;

            int downK = lowerA - lowerB; // the k-line to start the forward search
            int upK = upperA - upperB; // the k-line to start the reverse search

            int delta = (upperA - lowerA) - (upperB - lowerB);
            bool oddDelta = (delta & 1) != 0;

            // vector for the (0,0) to (x,y) search
            var downVector = new int[2 * max + 2];

            // vector for the (u,v) to (N,M) search
            var upVector = new int[2 * max + 2];

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
        /// Scan the tables of which lines are inserted and deleted,
        /// producing an edit script in forward order.
        /// 
        /// Created by Matthias Hertel, see http://www.mathertel.de
        /// This work is licensed under a Creative Commons Attribution 2.0 Germany License.
        /// see http://creativecommons.org/licenses/by/2.0/de/
        /// </summary>
        /// <returns>DiffItems array.</returns>
        public DiffItem[] CreateDiffs()
        {
            Lcs(0, dataA.Length, 0, dataB.Length);

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
    }
}
