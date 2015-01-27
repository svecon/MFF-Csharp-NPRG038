using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DiffAlgorithmTests")]

namespace DiffAlgorithm
{
    public class DiffHelper
    {
        Dictionary<string, int> hashedLines;
        public int FileANumberOfLines;
        public int FileBNumberOfLines;

        private readonly bool trimSpace;
        private readonly bool ignoreSpace;
        private readonly bool ignoreCase;

        public DiffHelper(bool trimSpace = false, bool ignoreSpace = false, bool ignoreCase = false)
        {
            this.trimSpace = trimSpace;
            this.ignoreCase = ignoreCase;
            this.ignoreSpace = ignoreSpace;
        }

        public DiffItem[] DiffText(string textA, string textB)
        {
            hashedLines = new Dictionary<string, int>(textA.Length + textB.Length);

            var dataA = new DiffData(hashStringLines(textA));
            var dataB = new DiffData(hashStringLines(textB));

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

        public Diff DiffFiles(FileInfo fileA, FileInfo fileB)
        {
            using (StreamReader streamA = fileA.OpenText())
            {
                using (StreamReader streamB = fileB.OpenText())
                {
                    hashedLines = new Dictionary<string, int>();

                    var diff = new Diff(fileA, fileB);


                    var dataA = new DiffData(hashStringLines(streamA.ReadToEnd()));
                    var dataB = new DiffData(hashStringLines(streamB.ReadToEnd()));

                    FileANumberOfLines = dataA.Length;
                    FileBNumberOfLines = dataB.Length;

                    hashedLines.Clear();

                    var da = new DiffAlgorithm(dataA, dataB);
                    diff.SetDiffItems(da.CreateDiffs());

                    return diff;
                }
            }
        }

        private int[] hashStringLines(string aText)
        {
            // get all codes of the text
            int lastUsedCode = hashedLines.Count;

            // strip off all cr, only use lf as textline separator.
            aText = aText.Replace("\r", "");
            var lines = aText.Split('\n');

            var codes = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                var s = applyOptions(lines[i], trimSpace, ignoreSpace, ignoreCase);

                if (hashedLines.TryGetValue(s, out codes[i])) continue;

                codes[i] = hashedLines[s] = ++lastUsedCode;
            }
            return codes;
        }

        private string applyOptions(string line, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            if (trimSpace)
                line = line.Trim();

            if (ignoreSpace)
                line = removeAllBlanks(line);

            if (ignoreCase)
                line = line.ToLower();

            return line;
        }

        private string removeAllBlanks(string line)
        {
            // TODO: optimization: faster blank removal.
            return Regex.Replace(line, "\\s+", " ");
        }

    }
}
