using System;
using System.IO;

namespace DiffAlgorithm
{
    /// <summary>
    /// Container for holding all information about a two-way diff.
    /// </summary>
    public class Diff
    {
        /// <summary>
        /// Number of lines for two diffed files.
        /// </summary>
        public struct FilesLineCountStruct
        {
            public int Old;
            public int New;
        }

        /// <summary>
        /// Do the files have an empty line at the end?
        /// (Standard in Unix files)
        /// </summary>
        public struct FilesEndsWithNewLineStruct
        {
            public bool Old;
            public bool New;
        }

        /// <summary>
        /// All DiffItems.
        /// </summary>
        public DiffItem[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;

        public FilesLineCountStruct FilesLineCount;
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        /// <summary>
        /// Constructor for Diff.
        /// </summary>
        /// <param name="oldFile">Old file diffed.</param>
        /// <param name="newFile">New file diffed.</param>
        public Diff(FileInfo oldFile, FileInfo newFile)
        {
            this.oldFile = oldFile;
            this.newFile = newFile;

            diffedTime = new DateTime();
            FilesLineCount = new FilesLineCountStruct();
            FilesEndsWithNewLine = new FilesEndsWithNewLineStruct();
        }

        /// <summary>
        /// Insert calculated diff items into the container.
        /// </summary>
        /// <param name="diffItems">Diff item changes between the files.</param>
        public void SetDiffItems(DiffItem[] diffItems)
        {
            Items = diffItems;
        }

    }
}
