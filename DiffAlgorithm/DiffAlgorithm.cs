using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffAlgorithm
{
    class DiffAlgorithm
    {
        private DiffData DataA;
        private DiffData DataB;

        public DiffAlgorithm(DiffData DataA, DiffData DataB)
        {
            this.DataA = DataA;
            this.DataB = DataB;
        }

        private void LCS(int LowerA, int UpperA, int LowerB, int UpperB)
        {
            // Fast walkthrough equal lines at the start
            while (LowerA < UpperA && LowerB < UpperB && DataA.data[LowerA] == DataB.data[LowerB])
            {
                LowerA++; LowerB++;
            }

            // Fast walkthrough equal lines at the end
            while (LowerA < UpperA && LowerB < UpperB && DataA.data[UpperA - 1] == DataB.data[UpperB - 1])
            {
                --UpperA; --UpperB;
            }

            if (LowerA == UpperA)
            {
                // mark as inserted lines.
                while (LowerB < UpperB)
                    DataB.modified[LowerB++] = true;

            } else if (LowerB == UpperB)
            {
                // mark as deleted lines.
                while (LowerA < UpperA)
                    DataA.modified[LowerA++] = true;

            } else
            {
                // Find the middle snakea and length of an optimal path for A and B
                SMSPoint snake = SMS(DataA, LowerA, UpperA, DataB, LowerB, UpperB);

                // The path is from LowerX to (x,y) and (x,y) ot UpperX
                LCS(LowerA, snake.x, LowerB, snake.y);
                LCS(snake.x, UpperA, snake.y, UpperB);
            }
        }

        private SMSPoint SMS(DiffData DataA, int LowerA, int UpperA, DiffData DataB, int LowerB, int UpperB)
        {
            int MAX = DataA.Length + DataB.Length + 1;

            int DownK = LowerA - LowerB; // the k-line to start the forward search
            int UpK = UpperA - UpperB; // the k-line to start the reverse search

            int Delta = (UpperA - LowerA) - (UpperB - LowerB);
            bool oddDelta = (Delta & 1) != 0;

            /// vector for the (0,0) to (x,y) search
            int[] DownVector = new int[2 * MAX + 2];

            /// vector for the (u,v) to (N,M) search
            int[] UpVector = new int[2 * MAX + 2];

            // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
            // and are access using diffItemsList specific offset: UpOffset UpVector and DownOffset for DownVektor
            int DownOffset = MAX - DownK;
            int UpOffset = MAX - UpK;

            int MaxD = ((UpperA - LowerA + UpperB - LowerB) / 2) + 1;

            // init vectors
            DownVector[DownOffset + DownK + 1] = LowerA;
            UpVector[UpOffset + UpK - 1] = UpperA;

            for (int D = 0; D <= MaxD; D++)
            {

                // Extend the forward path.
                for (int k = DownK - D; k <= DownK + D; k += 2)
                {
                    // find the only or better starting point
                    int x, y;
                    if (k == DownK - D)
                    {
                        x = DownVector[DownOffset + k + 1]; // down
                    } else
                    {
                        x = DownVector[DownOffset + k - 1] + 1; // diffItemsList step to the right
                        if ((k < DownK + D) && (DownVector[DownOffset + k + 1] >= x))
                            x = DownVector[DownOffset + k + 1]; // down
                    }
                    y = x - k;

                    // find the end of the furthest reaching forward D-path in diagonal k.
                    while ((x < UpperA) && (y < UpperB) && (DataA.data[x] == DataB.data[y]))
                    {
                        x++; y++;
                    }
                    DownVector[DownOffset + k] = x;

                    // overlap ?
                    if (oddDelta && (UpK - D < k) && (k < UpK + D))
                    {
                        if (UpVector[UpOffset + k] <= DownVector[DownOffset + k])
                        {
                            return new SMSPoint(DownVector[DownOffset + k], DownVector[DownOffset + k] - k);
                        }
                    }

                }

                // Extend the reverse path.
                for (int k = UpK - D; k <= UpK + D; k += 2)
                {
                    // find the only or better starting point
                    int x, y;
                    if (k == UpK + D)
                    {
                        x = UpVector[UpOffset + k - 1]; // up
                    } else
                    {
                        x = UpVector[UpOffset + k + 1] - 1; // left
                        if ((k > UpK - D) && (UpVector[UpOffset + k - 1] < x))
                            x = UpVector[UpOffset + k - 1]; // up
                    }
                    y = x - k;

                    while ((x > LowerA) && (y > LowerB) && (DataA.data[x - 1] == DataB.data[y - 1]))
                    {
                        x--; y--; // diagonal
                    }
                    UpVector[UpOffset + k] = x;

                    // overlap ?
                    if (!oddDelta && (DownK - D <= k) && (k <= DownK + D))
                    {
                        if (UpVector[UpOffset + k] <= DownVector[DownOffset + k])
                        {
                            return new SMSPoint(DownVector[DownOffset + k], DownVector[DownOffset + k] - k);
                        }
                    }

                }

            }

            throw new ApplicationException("The algorithm should never come here.");
        }

        public Item[] CreateDiffs()
        {
            LCS(0, DataA.Length, 0, DataB.Length);

            List<Item> diffItemsList = new List<Item>();
            int StartA, StartB;
            int LineA, LineB;

            LineA = 0;
            LineB = 0;
            while (LineA < DataA.Length || LineB < DataB.Length)
            {
                if ((LineA < DataA.Length) && (!DataA.modified[LineA])
                  && (LineB < DataB.Length) && (!DataB.modified[LineB]))
                {
                    // equal lines
                    LineA++;
                    LineB++;

                } else
                {
                    // maybe deleted and/or inserted lines
                    StartA = LineA;
                    StartB = LineB;

                    while (LineA < DataA.Length && (LineB >= DataB.Length || DataA.modified[LineA]))
                        LineA++;

                    while (LineB < DataB.Length && (LineA >= DataA.Length || DataB.modified[LineB]))
                        LineB++;

                    if ((StartA < LineA) || (StartB < LineB))
                    {
                        // store diffItemsList new difference-item
                        diffItemsList.Add(new Item(StartA, StartB, LineA - StartA, LineB - StartB));
                    }
                }
            }

            return diffItemsList.ToArray();
        }
    }
}
