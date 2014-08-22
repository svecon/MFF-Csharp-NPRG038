using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CoreLibrary.Exceptions
{
    class LeftDirectoryNotFoundException : LocationNotFoundException
    {
        public LeftDirectoryNotFoundException(DirectoryInfo info) : base(info)
        {
        }
    }
}
