using System.IO;
using System.Linq;

namespace CoreLibrary.Helpers
{
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


        public static bool IsTextFile(this string path)
        {
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

        public static bool IsTextFile(this FileInfo info)
        {
            return info.FullName.IsTextFile();
        }

    }
}
