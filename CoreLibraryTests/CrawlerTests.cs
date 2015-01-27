using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using CoreLibrary.FilesystemTree;

namespace CoreLibraryTests
{
    [TestClass]
    public class CrawlerTests
    {

        [ClassInitialize]
        public static void CreateFileStructures(TestContext context)
        {
            createRandomFile("2way/l/different.txt");
            createRandomFile("2way/r/different.txt");

            createRandomFile("2way/l/only_left.txt");
            createRandomFile("2way/r/only_right.txt");

            FileInfo same = createRandomFile("2way/l/same.txt");
            same.CopyTo(getTempPath() + "2way/r/same.txt", true);

            Directory.CreateDirectory(getTempPath() + "2way/l/empty_dir");
            Directory.CreateDirectory(getTempPath() + "2way/r/empty_dir");

            createRandomFile("2way/r/dir_different/different_file_in_different_folder.txt");

            createRandomFile("2way/l/dir/different_size.txt", 256);
            createRandomFile("2way/r/dir/different_size.txt", 128);
        }

        [ClassCleanup]
        public static void DeleteDileStructures()
        {
            //Directory.Delete(getTempPath(), true);
        }

        private static string getTempPath()
        {
            return Path.GetTempPath() + "crawlerTests\\";
        }

        private static FileInfo createRandomFile(string fileName, int sizeInKb = 128)
        {
            // Note: block size must be a factor of 1MB to avoid rounding errors :)
            const int blockSize = 256;
            const int blocksPerKb = (1024) / blockSize;
            var data = new byte[blockSize];
            var rng = new Random();

            fileName = getTempPath() + fileName;

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
            var crawler = new Crawler(getTempPath() + "2way/l", getTempPath() + "2way/r");

            Assert.AreEqual(CoreLibrary.Enums.DiffModeEnum.TwoWay, crawler.FilesystemDiff.DiffMode);

            FilesystemTree filesystem = crawler.FilesystemDiff;

            Assert.AreEqual(0, filesystem.Root.Files.Count);
            Assert.AreEqual(0, filesystem.Root.Directories.Count);

            Assert.AreEqual((int)CoreLibrary.Enums.LocationCombinationsEnum.OnLeftRight, filesystem.Root.Location);

            crawler.TraverseTree();

            Assert.AreEqual(3, filesystem.Root.Directories.Count);
            Assert.AreEqual(4, filesystem.Root.Files.Count);
        }
    }
}
