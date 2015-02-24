using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DiffAlgorithmTests")]

namespace DiffAlgorithm
{
    public class DiffHelper
    {
        private class SmartLineReader
        {
            private readonly FileInfo file;
            private char[] buffer;
            private int bufferCount;
            private bool endedWithNewline = true;

            public SmartLineReader(FileInfo fileInfo)
            {
                file = fileInfo;

                buffer = new char[2 * 1024];
                bufferCount = 0;
            }

            public bool EndedWithNewLine()
            {
                return endedWithNewline;
            }

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
                            continue;
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

                if (sb.Length > 0)
                {
                    endedWithNewline = false;
                    yield return sb.ToString();
                }

                fileReader.Close();
            }
        }

        Dictionary<string, int> hashedLines;

        private readonly bool trimSpace;
        private readonly bool ignoreSpace;
        private readonly bool ignoreCase;

        public DiffHelper(bool trimSpace = false, bool ignoreSpace = false, bool ignoreCase = false)
        {
            this.trimSpace = trimSpace;
            this.ignoreCase = ignoreCase;
            this.ignoreSpace = ignoreSpace;
        }

        public DiffItem[] DiffText(string oldText, string newText)
        {
            hashedLines = new Dictionary<string, int>(oldText.Length + newText.Length);

            var oldData = new DiffData(hashStringLines(oldText));
            var newData = new DiffData(hashStringLines(newText));

            hashedLines.Clear();

            var da = new DiffAlgorithm(oldData, newData);
            return da.CreateDiffs();
        }

        public DiffItem[] DiffInt(int[] oldArray, int[] newArray)
        {
            var dataA = new DiffData(oldArray);
            var dataB = new DiffData(newArray);

            var da = new DiffAlgorithm(dataA, dataB);
            return da.CreateDiffs();
        }

        public Diff DiffFiles(FileInfo oldFile, FileInfo newFile)
        {
            var diff = new Diff(oldFile, newFile);

            hashedLines = new Dictionary<string, int>();

            var oldFileReader = new SmartLineReader(oldFile);
            var newFileReader = new SmartLineReader(newFile);

            var oldData = new DiffData(hashStringLines(oldFileReader));
            var newData = new DiffData(hashStringLines(newFileReader));
            hashedLines.Clear();

            diff.FilesLineCount.Old = oldData.Length;
            diff.FilesLineCount.New = newData.Length;

            diff.FilesEndsWithNewLine.Old = oldFileReader.EndedWithNewLine();
            diff.FilesEndsWithNewLine.New = newFileReader.EndedWithNewLine();

            var da = new DiffAlgorithm(oldData, newData);
            diff.SetDiffItems(da.CreateDiffs());

            return diff;
        }

        public Diff3 DiffFiles(FileInfo oldFile, FileInfo newFile, FileInfo hisFile)
        {
            var diff = new Diff3(oldFile, newFile, hisFile);

            hashedLines = new Dictionary<string, int>();

            var oldFileReader = new SmartLineReader(oldFile);
            var newFileReader = new SmartLineReader(newFile);
            var hisFileReader = new SmartLineReader(hisFile);

            var oldData = new DiffData(hashStringLines(oldFileReader));
            var newData = new DiffData(hashStringLines(newFileReader));
            var hisData = new DiffData(hashStringLines(hisFileReader));
            hashedLines.Clear();

            diff.FilesLineCount.Old = oldData.Length;
            diff.FilesLineCount.New = newData.Length;
            diff.FilesLineCount.His = hisData.Length;

            diff.FilesEndsWithNewLine.Old = oldFileReader.EndedWithNewLine();
            diff.FilesEndsWithNewLine.New = newFileReader.EndedWithNewLine();
            diff.FilesEndsWithNewLine.His = hisFileReader.EndedWithNewLine();

            var da = new DiffAlgorithm(oldData, newData);
            var da2 = new DiffAlgorithm(oldData, hisData);

            var d3a = new Diff3Algorithm(da.CreateDiffs(), da2.CreateDiffs(), newData.Data, hisData.Data);

            var x = d3a.Parse();

            return diff;
        }

        private int[] hashStringLines(string text)
        {
            // get all codes of the text
            int lastUsedCode = hashedLines.Count;

            // strip off all cr, only use lf as textline separator.
            text = text.Replace("\r", "");
            string[] lines = text.Split('\n');

            var codes = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = applyOptions(lines[i], trimSpace, ignoreSpace, ignoreCase);

                if (hashedLines.TryGetValue(s, out codes[i])) continue;

                codes[i] = hashedLines[s] = ++lastUsedCode;
            }
            return codes;
        }



        private int[] hashStringLines(SmartLineReader fileReader)
        {
            int lastUsedCode = hashedLines.Count;
            var codes = new List<int>();

            foreach (string line in fileReader.IterateLines())
            {
                string mline = applyOptions(line, trimSpace, ignoreSpace, ignoreCase);

                int x;
                if (hashedLines.TryGetValue(mline, out x))
                {
                    codes.Add(x);
                    continue;
                }

                hashedLines[line] = ++lastUsedCode;
                codes.Add(lastUsedCode);
            }

            return codes.ToArray();
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
