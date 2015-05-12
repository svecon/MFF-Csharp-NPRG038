using System;
using System.IO;

namespace TextDiffAlgorithm.ThreeWay
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
            /// <summary>
            /// Number of lines in base file.
            /// </summary>
            public int Base;

            /// <summary>
            /// Number of lines in local file.
            /// </summary>
            public int Local;

            /// <summary>
            /// Number of lines in remote file.
            /// </summary>
            public int Remote;
        }

        /// <summary>
        /// Do the files have an empty line at the end?
        /// (Standard in Unix files)
        /// </summary>
        public struct FilesEndsWithNewLineStruct
        {
            /// <summary>
            /// Does base file end with new line?
            /// </summary>
            public bool Base;

            /// <summary>
            /// Does local file end with new line?
            /// </summary>
            public bool Local;

            /// <summary>
            /// Does remote file end with new line?
            /// </summary>
            public bool Remote;
        }

        /// <summary>
        /// All DiffItems.
        /// </summary>
        public Diff3Item[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;
        private FileInfo hisFile;

        /// <summary>
        /// Structure for storing number of lines in each file.
        /// </summary>
        public FilesLineCountStruct FilesLineCount;

        /// <summary>
        /// Structure for storing whether files end with new line.
        /// </summary>
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        /// <summary>
        /// Initializes new instance of the <see cref="Diff3"/>
        /// </summary>
        /// <param name="oldFile">Local file diffed.</param>
        /// <param name="newFile">Remote file diffed.</param>
        /// <param name="hisFile">Remote new file diffed.</param>
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
            FilesLineCount.Base = oldLineCount;
            FilesLineCount.Local = newLineCount;
            FilesLineCount.Remote = hisLineCount;

            FilesEndsWithNewLine.Base = oldEndsNewLine;
            FilesEndsWithNewLine.Local = newEndsNewLine;
            FilesEndsWithNewLine.Remote = hisEndsNewLine;
        }

    }
}
