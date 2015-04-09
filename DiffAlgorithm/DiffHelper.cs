using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using DiffAlgorithm.ThreeWay;
using DiffAlgorithm.TwoWay;

[assembly: InternalsVisibleTo("DiffAlgorithmTests")]

namespace DiffAlgorithm
{
    /// <summary>
    /// DiffHelper class makes it easier to diff whole files.
    /// </summary>
    public class DiffHelper
    {

        /// <summary>
        /// SmarLineReader offers some advanced functionality important during diffing.
        /// </summary>
        private class SmartLineReader
        {
            private readonly FileInfo file;
            private readonly char[] buffer;
            private int bufferCount;
            private bool endedWithNewline = true;

            /// <summary>
            /// Contructor of SmarLineReader.
            /// </summary>
            /// <param name="fileInfo">FileInfo of a file to be read.</param>
            public SmartLineReader(FileInfo fileInfo)
            {
                file = fileInfo;

                buffer = new char[2 * 1024];
                bufferCount = 0;
            }

            /// <summary>
            /// Did the file end with a new s character?
            /// </summary>
            /// <returns></returns>
            public bool DoesFileEndWithNewLine()
            {
                return endedWithNewline;
            }

            /// <summary>
            /// Iterating over the lines of the file.
            /// </summary>
            /// <returns>Enumerable lines.</returns>
            public IEnumerable<string> IterateLines()
            {
                StreamReader fileReader = file.OpenText();
                var sb = new StringBuilder();
                while (!fileReader.EndOfStream)
                {
                    bufferCount = fileReader.ReadBlock(buffer, 0, buffer.Length);

                    for (int i = 0; i < bufferCount; i++)
                    {
                        if (buffer[i] == '\r')
                        {
                            // do nothing
                        } else if (buffer[i] == '\n')
                        {
                            yield return sb.Append('\n').ToString();
                            sb.Clear();
                        } else
                        {
                            sb.Append(buffer[i]);
                        }
                    }
                }

                // is there something left is the buffer?
                if (sb.Length > 0)
                {
                    endedWithNewline = false;
                    yield return sb.ToString();
                }

                fileReader.Close();
            }
        }

        /// <summary>
        /// Dictionary of all different lines of the files.
        /// Used to cut down the memory usage.
        /// </summary>
        Dictionary<string, int> hashedLines;

        private readonly bool trimSpace;
        private readonly bool ignoreSpace;
        private readonly bool ignoreCase;

        /// <summary>
        /// todo settings somewhere global
        /// </summary>
        /// <param name="trimSpace">Trim white-space?</param>
        /// <param name="ignoreSpace">Ignore all white-space?</param>
        /// <param name="ignoreCase">Case-insensitive?</param>
        public DiffHelper(bool trimSpace = false, bool ignoreSpace = false, bool ignoreCase = false)
        {
            this.trimSpace = trimSpace;
            this.ignoreCase = ignoreCase;
            this.ignoreSpace = ignoreSpace;
        }

        /// <summary>
        /// Diffs two strings
        /// </summary>
        /// <param name="oldText">Local string</param>
        /// <param name="newText">Remote String</param>
        /// <returns>DiffItem[] showing the changes between the two strings.</returns>
        public DiffItem[] DiffText(string oldText, string newText)
        {
            hashedLines = new Dictionary<string, int>(oldText.Length + newText.Length);

            var oldData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(oldText));
            var newData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(newText));

            hashedLines.Clear();

            var da = new TwoWay.DiffAlgorithm(oldData, newData);
            return da.RunAndCreateDiffs();
        }

        /// <summary>
        /// Diffs two int arrays.
        /// </summary>
        /// <param name="oldArray">Local array to be diffed.</param>
        /// <param name="newArray">Remote array to be diffed.</param>
        /// <returns>DiffItem[] showing the changes between the two arrays.</returns>
        public DiffItem[] DiffInt(int[] oldArray, int[] newArray)
        {
            var dataA = new TwoWay.DiffAlgorithm.DiffData(oldArray);
            var dataB = new TwoWay.DiffAlgorithm.DiffData(newArray);

            var da = new TwoWay.DiffAlgorithm(dataA, dataB);
            return da.RunAndCreateDiffs();
        }

        /// <summary>
        /// Diffs three texts.
        /// </summary>
        /// <param name="oldText">Local text.</param>
        /// <param name="newText">My new text.</param>
        /// <param name="hisText">Remote new text.</param>
        /// <returns>Diff3Item[] showing the differences between three texts.</returns>
        public Diff3Item[] DiffText(string oldText, string newText, string hisText)
        {
            hashedLines = new Dictionary<string, int>();

            var oldData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(oldText));
            var newData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(newText));
            var hisData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(hisText));
            hashedLines.Clear();

            var da = new TwoWay.DiffAlgorithm(oldData, newData);
            var da2 = new TwoWay.DiffAlgorithm(oldData, hisData);

            var d3A = new Diff3Algorithm(da.RunAndCreateDiffs(), da2.RunAndCreateDiffs(true), newData.Data, hisData.Data);

            return d3A.MergeIntoDiff3Chunks();
        }

        /// <summary>
        /// Diffs two files.
        /// </summary>
        /// <param name="oldFile">Local file to be diffed.</param>
        /// <param name="newFile">Remote file to be diffed.</param>
        /// <returns>DiffItem[] showing the changes between the two files.</returns>
        public Diff DiffFiles(FileInfo oldFile, FileInfo newFile)
        {
            var diff = new Diff(oldFile, newFile);

            hashedLines = new Dictionary<string, int>();

            var oldFileReader = new SmartLineReader(oldFile);
            var newFileReader = new SmartLineReader(newFile);

            var oldData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(oldFileReader));
            var newData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(newFileReader));
            hashedLines.Clear();

            diff.SetStatistics(oldData.Length, newData.Length,
                oldFileReader.DoesFileEndWithNewLine(), newFileReader.DoesFileEndWithNewLine());
            
            var da = new TwoWay.DiffAlgorithm(oldData, newData);
            diff.SetDiffItems(da.RunAndCreateDiffs());

            return diff;
        }

        /// <summary>
        /// Diffs three files.
        /// </summary>
        /// <param name="oldFile">Local file.</param>
        /// <param name="newFile">My new file.</param>
        /// <param name="hisFile">Remote new file.</param>
        /// <returns>Diff3 container with all changes between three files.</returns>
        public Diff3 DiffFiles(FileInfo oldFile, FileInfo newFile, FileInfo hisFile)
        {
            var diff = new Diff3(oldFile, newFile, hisFile);

            hashedLines = new Dictionary<string, int>();

            var oldFileReader = new SmartLineReader(oldFile);
            var newFileReader = new SmartLineReader(newFile);
            var hisFileReader = new SmartLineReader(hisFile);

            var oldData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(oldFileReader));
            var newData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(newFileReader));
            var hisData = new TwoWay.DiffAlgorithm.DiffData(HashStringLines(hisFileReader));
            hashedLines.Clear();

            diff.SetStatistics(oldData.Length, newData.Length, hisData.Length,
                oldFileReader.DoesFileEndWithNewLine(), newFileReader.DoesFileEndWithNewLine(), hisFileReader.DoesFileEndWithNewLine());

            var da = new TwoWay.DiffAlgorithm(oldData, newData);
            var da2 = new TwoWay.DiffAlgorithm(oldData, hisData);

            var d3A = new Diff3Algorithm(da.RunAndCreateDiffs(), da2.RunAndCreateDiffs(true), newData.Data, hisData.Data);
            diff.SetDiffItems(d3A.MergeIntoDiff3Chunks());

            return diff;
        }

        /// <summary>
        /// Hashing all lines from the file into numbers.
        /// </summary>
        /// <param name="text">Text that we want to hash into int array line by line.</param>
        /// <returns>Array of hashed lines into numbers.</returns>
        private int[] HashStringLines(string text)
        {
            // get all codes of the text
            int lastUsedCode = hashedLines.Count;

            // strip off all cr, only use lf as textline separator.
            text = text.Replace("\r", "");
            string[] lines = text.Split('\n');

            var codes = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = ApplyTransformations(lines[i]);

                if (hashedLines.TryGetValue(s, out codes[i])) continue;

                codes[i] = hashedLines[s] = ++lastUsedCode;
            }
            return codes;
        }

        /// <summary>
        /// Hashing all lines from the file into numbers.
        /// </summary>
        /// <param name="fileReader">SmartLineReader which the lines are read from.</param>
        /// <returns>Array of hashed lines into numbers.</returns>
        private int[] HashStringLines(SmartLineReader fileReader)
        {
            int lastUsedCode = hashedLines.Count;
            var codes = new List<int>();

            foreach (string line in fileReader.IterateLines())
            {
                string mline = ApplyTransformations(line);

                int x;
                if (hashedLines.TryGetValue(mline, out x))
                {
                    codes.Add(x);
                    continue;
                }

                hashedLines[mline] = ++lastUsedCode;
                codes.Add(lastUsedCode);
            }

            return codes.ToArray();
        }

        /// <summary>
        /// Method for applying all different settings and modifications to a text before it is diffed.
        /// </summary>
        /// <param name="s">String to be modified.</param>
        /// <param name="trimSpace">Trim white-space?</param>
        /// <param name="ignoreSpace">Ignore all white-space?</param>
        /// <param name="ignoreCase">Case-insensitive?</param>
        /// <returns>Modified string</returns>
        private string ApplyTransformations(string s)
        {
            // TODO: GLOBAL CONFIG???????
            if (trimSpace)
                s = s.Trim();

            if (ignoreSpace)
                s = RemoveAllBlanks(s);

            if (ignoreCase)
                s = s.ToLower();

            return s;
        }

        /// <summary>
        /// Removing all the blanks in the string
        /// </summary>
        /// <param name="s">String to be modified.</param>
        /// <returns>String without any white-space.</returns>
        private string RemoveAllBlanks(string s)
        {
            // TODO: optimization: faster blank removal.
            return Regex.Replace(s, "\\s+", " ");
        }

    }
}
