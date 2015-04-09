using System;
using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreLibraryTests
{
    [TestClass]
    public class CrawlerTests
    {

        [ClassInitialize]
        public static void CreateFileStructures(TestContext context)
        {
            CreateRandomFile("2way/l/different.txt");
            CreateRandomFile("2way/r/different.txt");

            CreateRandomFile("2way/l/only_left.txt");
            CreateRandomFile("2way/r/only_right.txt");

            FileInfo same = CreateRandomFile("2way/l/same.txt");
            same.CopyTo(GetTempPath() + "2way/r/same.txt", true);

            Directory.CreateDirectory(GetTempPath() + "2way/l/empty_dir");
            Directory.CreateDirectory(GetTempPath() + "2way/r/empty_dir");

            CreateRandomFile("2way/r/dir_different/different_file_in_different_folder.txt");

            CreateRandomFile("2way/l/dir/different_size.txt", 256);
            CreateRandomFile("2way/r/dir/different_size.txt", 128);
        }

        [ClassCleanup]
        public static void DeleteDileStructures()
        {
            //Directory.Delete(getTempPath(), true);
        }

        private static string GetTempPath()
        {
            return Path.GetTempPath() + "crawlerTests\\";
        }

        private static FileInfo CreateRandomFile(string fileName, int sizeInKb = 128)
        {
            // Note: block size must be a factor of 1MB to avoid rounding errors :)
            const int blockSize = 256;
            const int blocksPerKb = (1024) / blockSize;
            var data = new byte[blockSize];
            var rng = new Random();

            fileName = GetTempPath() + fileName;

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (FileStream stream = File.OpenWrite(fileName))
            {
                for (int i = 0; i < sizeInKb * blocksPerKb; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }

            return new FileInfo(fileName);
        }

        [TestMethod]
        public void CrawlerTestsBasic()
        {
            var crawler = new Crawler().InitializeCrawler(GetTempPath() + "2way/l", GetTempPath() + "2way/r");

            IFilesystemTree filesystemTree = crawler.TraverseTree();

            Assert.AreEqual(DiffModeEnum.TwoWay, filesystemTree.DiffMode);

            Assert.AreEqual((int)LocationCombinationsEnum.OnLocalRemote, filesystemTree.Root.Location);

            Assert.AreEqual(3, filesystemTree.Root.Directories.Count);
            Assert.AreEqual(4, filesystemTree.Root.Files.Count);
        }
    }
}
