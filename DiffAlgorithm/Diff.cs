using System;
using System.IO;

namespace DiffAlgorithm
{
    public class Diff
    {
        public struct FilesLineCountStruct
        {
            public int Old;
            public int New;
        }

        public struct FilesEndsWithNewLineStruct
        {
            public bool Old;
            public bool New;
        }

        public DiffItem[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;

        public FilesLineCountStruct FilesLineCount;
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        public Diff(FileInfo oldFile, FileInfo newFile)
        {
            this.oldFile = oldFile;
            this.newFile = newFile;

            diffedTime = new DateTime();
            FilesLineCount = new FilesLineCountStruct();
            FilesEndsWithNewLine = new FilesEndsWithNewLineStruct();
        }

        public void SetDiffItems(DiffItem[] diffItems)
        {
            Items = diffItems;
        }

    }
}
