using System.IO;
using System.Linq;

namespace CoreLibrary.Helpers
{
    /// <summary>
    /// Helper for checking whether the file is a text file or a binary file.
    /// 
    /// Binary files have a lot of consecutive zero bytes.
    /// </summary>
    public static class TextFileChecker
    {
        /// <summary>
        /// Number of bytes to read from the begining of a file.
        /// </summary>
        private const int READ_BYTES = 256;

        /// <summary>
        /// Number of consecutive zeroe bytes that imply binary file.
        /// </summary>
        private const int CONSECUTIVE_THRESHOLD = 4;

        /// <summary>
        /// Total number of zero bytes that imply binary file.
        /// </summary>
        private const int ZEROES_THRESHOLD = READ_BYTES / 2;

        /// <summary>
        /// Checks the file to be a text file or a binary file.
        /// </summary>
        /// <param name="path">Path to the file that will be checked.</param>
        /// <returns>True when the file is text file.</returns>
        public static bool IsTextFile(this string path)
        {
            if (!File.Exists(path))
                return false;

            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                int numberOfZeroBytes = 0;
                int maxConsecutiveZeroBytes = 0;
                int currentConsecutiveZeroBytes = 0;

                foreach (byte b in reader.ReadBytes(READ_BYTES).Where(b => b == 0))
                {
                    currentConsecutiveZeroBytes++;
                    numberOfZeroBytes++;

                    if (currentConsecutiveZeroBytes > maxConsecutiveZeroBytes)
                    {
                        maxConsecutiveZeroBytes = currentConsecutiveZeroBytes;
                    }
                }

                return maxConsecutiveZeroBytes < CONSECUTIVE_THRESHOLD && numberOfZeroBytes < ZEROES_THRESHOLD;
            }

        }

        /// <summary>
        /// A helper method for FileInfo.
        /// </summary>
        /// <param name="info">FileInfo of the file to be checked.</param>
        /// <returns>True when the file is text file.</returns>
        public static bool IsTextFile(this FileInfo info)
        {
            return info.FullName.IsTextFile();
        }

    }
}
