using System;
using System.IO;

namespace DiffAlgorithm
{
    public class Diff3
    {
        public struct FilesLineCountStruct
        {
            public int Old;
            public int New;
            public int His;
        }

        public struct FilesEndsWithNewLineStruct
        {
            public bool Old;
            public bool New;
            public bool His;
        }

        public DiffItem[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;
        private FileInfo hisFile;

        public FilesLineCountStruct FilesLineCount;
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        public Diff3(FileInfo oldFile, FileInfo newFile, FileInfo hisFile)
        {
            this.oldFile = oldFile;
            this.newFile = newFile;
            this.hisFile = hisFile;

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
