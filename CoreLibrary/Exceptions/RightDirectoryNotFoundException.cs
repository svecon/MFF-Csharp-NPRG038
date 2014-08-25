using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CoreLibrary.Exceptions
{
    class RightDirectoryNotFoundException : LocationNotFoundException
    {
        public RightDirectoryNotFoundException(FileSystemInfo info)
            : base(info)
        {
        }
    }
}
