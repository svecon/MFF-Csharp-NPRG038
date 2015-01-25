using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DiffAlgorithmTests")]

namespace DiffAlgorithm
{
    public class Diff
    {
        Dictionary<string, int> hashedLines;
        public int FileANumberOfLines;
        public int FileBNumberOfLines;

        public DiffItem[] DiffText(string textA, string textB, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            hashedLines = new Dictionary<string, int>(textA.Length + textB.Length);

            var dataA = new DiffData(HashStringLines(textA, trimSpace, ignoreSpace, ignoreCase));
            var dataB = new DiffData(HashStringLines(textB, trimSpace, ignoreSpace, ignoreCase));

            hashedLines.Clear();

            var da = new DiffAlgorithm(dataA, dataB);
            return da.CreateDiffs();
        }

        public DiffItem[] DiffInt(int[] arrayA, int[] arrayB)
        {
            var dataA = new DiffData(arrayA);
            var dataB = new DiffData(arrayB);

            var da = new DiffAlgorithm(dataA, dataB);
            return da.CreateDiffs();
        }

        public DiffItem[] DiffFiles(FileInfo fileA, FileInfo fileB)
        {
            using (StreamReader streamA = fileA.OpenText())
            {
                using (StreamReader streamB = fileB.OpenText())
                {
                    hashedLines = new Dictionary<string, int>();

                    var dataA = new DiffData(HashStringLines(streamA.ReadToEnd(), false, false, false));
                    var dataB = new DiffData(HashStringLines(streamB.ReadToEnd(), false, false, false));

                    FileANumberOfLines = dataA.Length;
                    FileBNumberOfLines = dataB.Length;

                    hashedLines.Clear();

                    var da = new DiffAlgorithm(dataA, dataB);
                    return da.CreateDiffs();
                }
            }
        }

        private int[] HashStringLines(string aText, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            // get all codes of the text
            int lastUsedCode = hashedLines.Count;

            // strip off all cr, only use lf as textline separator.
            aText = aText.Replace("\r", "");
            var lines = aText.Split('\n');

            var codes = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                var s = lines[i];
                if (trimSpace)
                    s = s.Trim();

                if (ignoreSpace)
                {
                    s = Regex.Replace(s, "\\s+", " ");            // TODO: optimization: faster blank removal.
                }

                if (ignoreCase)
                    s = s.ToLower();


                if (hashedLines.TryGetValue(s, out codes[i])) continue;

                lastUsedCode++;
                hashedLines[s] = lastUsedCode;
                codes[i] = lastUsedCode;
            }
            return codes;
        }

    }
}
