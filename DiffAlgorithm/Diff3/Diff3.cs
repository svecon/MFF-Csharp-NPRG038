using System;
using System.IO;

namespace DiffAlgorithm.Diff3
{
    /// <summary>
    /// Container for holding all information about a three-way diff.
    /// </summary>
    public class Diff3
    {
        /// <summary>
        /// Number of lines for three diffed files.
        /// </summary>
        public struct FilesLineCountStruct
        {
            public int Old;
            public int New;
            public int His;
        }

        /// <summary>
        /// Do the files have an empty line at the end?
        /// (Standard in Unix files)
        /// </summary>
        public struct FilesEndsWithNewLineStruct
        {
            public bool Old;
            public bool New;
            public bool His;
        }

        /// <summary>
        /// All DiffItems.
        /// </summary>
        public Diff3Item[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;
        private FileInfo hisFile;

        public FilesLineCountStruct FilesLineCount;
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        /// <summary>
        /// Constructor for Diff3.
        /// </summary>
        /// <param name="oldFile">Old file diffed.</param>
        /// <param name="newFile">New file diffed.</param>
        /// <param name="hisFile">His new file diffed.</param>
        public Diff3(FileInfo oldFile, FileInfo newFile, FileInfo hisFile)
        {
            this.oldFile = oldFile;
            this.newFile = newFile;
            this.hisFile = hisFile;

            diffedTime = DateTime.Now;
            FilesLineCount = new FilesLineCountStruct();
            FilesEndsWithNewLine = new FilesEndsWithNewLineStruct();
        }

        /// <summary>
        /// Insert calculated diff items into the container.
        /// </summary>
        /// <param name="diffItems">Diff item changes between the files.</param>
        public void SetDiffItems(Diff3Item[] diffItems)
        {
            Items = diffItems;
        }

        /// <summary>
        /// Sets statistics useful during diffing.
        /// 
        /// Number of lines.
        /// Does file end with new line?
        /// </summary>
        /// <param name="oldLineCount">Number of lines in the old file.</param>
        /// <param name="newLineCount">Number of files in the new file.</param>
        /// <param name="hisLineCount">Number of files in his file.</param>
        /// <param name="oldEndsNewLine">Does old file end with new line?</param>
        /// <param name="newEndsNewLine">Does new file end with new line?</param>
        /// <param name="hisEndsNewLine">Does his file end with new line?</param>
        public void SetStatistics(int oldLineCount, int newLineCount, int hisLineCount, bool oldEndsNewLine, bool newEndsNewLine, bool hisEndsNewLine)
        {
            FilesLineCount.Old = oldLineCount;
            FilesLineCount.New = newLineCount;
            FilesLineCount.His = hisLineCount;

            FilesEndsWithNewLine.Old = oldEndsNewLine;
            FilesEndsWithNewLine.New = newEndsNewLine;
            FilesEndsWithNewLine.His = hisEndsNewLine;
        }

    }
}
