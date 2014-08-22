using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CoreLibrary.Exceptions
{
    class BaseDirectoryNotFoundException : LocationNotFoundException
    {
        public BaseDirectoryNotFoundException(DirectoryInfo info) : base(info)
        {
        }
    }
}
