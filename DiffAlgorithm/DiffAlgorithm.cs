using System;
using System.Collections.Generic;

namespace DiffAlgorithm
{
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
        /// Resursive algorithm for LCS.
        /// </summary>
        /// <param name="lowerA">Lower bound for data A</param>
        /// <param name="upperA">Upper bound for data A</param>
        /// <param name="lowerB">Lower bound for data A</param>
        /// <param name="upperB">Upper bound for data B</param>
        private void LCS(int lowerA, int upperA, int lowerB, int upperB)
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
                SMSPoint snake = SMS(lowerA, upperA, lowerB, upperB);

                // The path is from LowerX to (x,y) and (x,y) ot UpperX
                LCS(lowerA, snake.x, lowerB, snake.y);
                LCS(snake.x, upperA, snake.y, upperB);
            }
        }

        private SMSPoint SMS(int lowerA, int upperA, int lowerB, int upperB)
        {
            int max = dataA.Length + dataB.Length + 1;

            int downK = lowerA - lowerB; // the k-line to start the forward search
            int upK = upperA - upperB; // the k-line to start the reverse search

            int delta = (upperA - lowerA) - (upperB - lowerB);
            bool oddDelta = (delta & 1) != 0;

            // vector for the (0,0) to (x,y) search
            int[] downVector = new int[2 * max + 2];

            // vector for the (u,v) to (N,M) search
            int[] upVector = new int[2 * max + 2];

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
                    int x, y;
                    if (k == downK - d)
                    {
                        x = downVector[downOffset + k + 1]; // down
                    } else
                    {
                        x = downVector[downOffset + k - 1] + 1; // diffItemsList step to the right
                        if ((k < downK + d) && (downVector[downOffset + k + 1] >= x))
                            x = downVector[downOffset + k + 1]; // down
                    }
                    y = x - k;

                    // find the end of the furthest reaching forward D-path in diagonal k.
                    while ((x < upperA) && (y < upperB) && (dataA.Data[x] == dataB.Data[y]))
                    {
                        x++; y++;
                    }
                    downVector[downOffset + k] = x;

                    // overlap ?
                    if (!oddDelta || (upK - d >= k) || (k >= upK + d)) continue;
                    if (upVector[upOffset + k] > downVector[downOffset + k]) continue;

                    return new SMSPoint(downVector[downOffset + k], downVector[downOffset + k] - k);
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

                    return new SMSPoint(downVector[downOffset + k], downVector[downOffset + k] - k);
                }

            }

            throw new ApplicationException("The algorithm should never come here.");
        }

        public DiffItem[] CreateDiffs()
        {
            LCS(0, dataA.Length, 0, dataB.Length);

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
