using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    /// <summary>
    /// Interface for a FilesystemTree structure that can accept a Visitor
    /// </summary>
    public interface IFilesystemTreeVisitable
    {
        void Accept(IFilesystemTreeVisitor visitor);
    }
}
