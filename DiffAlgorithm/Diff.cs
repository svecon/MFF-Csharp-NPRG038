using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DiffAlgorithmTests")]

namespace DiffAlgorithm
{
    public class Diff
    {
        Dictionary<string, int> hashedLines;

        public Item[] DiffText(string TextA, string TextB, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            hashedLines = new Dictionary<string, int>(TextA.Length + TextB.Length);

            // The A-Version of the data (original data) to be compared.
            DiffData DataA = new DiffData(HashStringLines(TextA, trimSpace, ignoreSpace, ignoreCase));

            // The B-Version of the data (modified data) to be compared.
            DiffData DataB = new DiffData(HashStringLines(TextB, trimSpace, ignoreSpace, ignoreCase));

            hashedLines.Clear();

            DiffAlgorithm da = new DiffAlgorithm(DataA, DataB);
            return da.CreateDiffs();
        }

        public Item[] DiffInt(int[] ArrayA, int[] ArrayB)
        {
            DiffData DataA = new DiffData(ArrayA);
            DiffData DataB = new DiffData(ArrayB);

            DiffAlgorithm da = new DiffAlgorithm(DataA, DataB);
            return da.CreateDiffs();
        }

        private int[] HashStringLines(string aText, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            // get all codes of the text
            string[] Lines;
            int[] Codes;
            int lastUsedCode = hashedLines.Count;
            string s;

            // strip off all cr, only use lf as textline separator.
            aText = aText.Replace("\r", "");
            Lines = aText.Split('\n');

            Codes = new int[Lines.Length];

            for (int i = 0; i < Lines.Length; ++i)
            {
                s = Lines[i];
                if (trimSpace)
                    s = s.Trim();

                if (ignoreSpace)
                {
                    s = Regex.Replace(s, "\\s+", " ");            // TODO: optimization: faster blank removal.
                }

                if (ignoreCase)
                    s = s.ToLower();


                if (!hashedLines.TryGetValue(s, out Codes[i]))
                {
                    lastUsedCode++;
                    hashedLines[s] = lastUsedCode;
                    Codes[i] = lastUsedCode;
                }
            }
            return Codes;
        }

    }
}
