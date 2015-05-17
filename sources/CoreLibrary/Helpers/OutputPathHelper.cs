using System.IO;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.Helpers
{
    /// <summary>
    /// Helper class for combining output paths
    /// </summary>
    public static class OutputPathHelper
    {
        /// <summary>
        /// Checks if given directory exists and creates it if does not
        /// </summary>
        /// <param name="path">Path to a directory</param>
        public static void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Creates a path for the given node and outputDirectory.
        /// </summary>
        /// <param name="node">Node that will requests the output path.</param>
        /// <param name="outputDirectory">Custom directory.</param>
        /// <returns>A string path</returns>
        public static string CreatePath(this INodeFileNode node, string outputDirectory)
        {
            if (outputDirectory == null)
            {
                return node.GetAbsolutePath(LocationEnum.OnBase);
            }

            string output = Directory.Exists(outputDirectory)
                ? Path.Combine(outputDirectory, node.Info.Name)
                : Path.Combine(Path.GetDirectoryName(node.Info.FullName) ?? string.Empty, outputDirectory);

            CheckAndCreateDirectory(Path.GetDirectoryName(output));

            return output;
        }
    }
}
