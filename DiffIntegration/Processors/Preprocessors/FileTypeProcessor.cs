using System.IO;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.Interfaces;
using CoreLibrary.Processors.Preprocessors;
using CoreLibrary.Settings.Attributes;

namespace DiffIntegration.Processors.Preprocessors
{
    /// <summary>
    /// Filter for C# source codes (manually typed ones).
    /// 
    /// Leaves out everything else.
    /// </summary>
    public class FileTypeProcessor : PreProcessorAbstract
    {
        public override DiffModeEnum Mode { get { return DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 300; } }

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

        public override void Process(IFilesystemTreeDirNode node)
        {
        }

        public override void Process(IFilesystemTreeFileNode node)
        {
            if (!CheckModeAndStatus(node))
                return;

            if (node.Status == NodeStatusEnum.IsIgnored)
                return;

            using (var reader = new BinaryReader(File.OpenRead(node.Info.FullName)))
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

                node.FileType = maxConsecutiveZeroBytes >= CONSECUTIVE_THRESHOLD
                                || numberOfZeroBytes >= ZEROES_THRESHOLD
                    ? FileTypeEnum.Binary
                    : FileTypeEnum.Text;
            }
        }
    }
}
