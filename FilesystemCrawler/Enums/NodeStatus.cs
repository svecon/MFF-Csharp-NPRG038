using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesystemCrawler.Enums
{
    public enum NodeStatus
    {
        Initial, HasError, IsIgnored, WasProcessed
    }
}
