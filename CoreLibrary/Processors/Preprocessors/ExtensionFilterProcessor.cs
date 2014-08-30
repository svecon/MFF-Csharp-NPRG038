using CoreLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Processors.Preprocessors
{
    class ExtensionFilterProcessor : AbstractPreProcessor
    {

        public override Enums.DiffModeEnum Mode { get { return Enums.DiffModeEnum.ThreeWay; } }

        public override int Priority { get { return 100; } }

        public override void Process(IFilesystemTreeDirNode node)
        {

        }

        public override void Process(IFilesystemTreeFileNode node)
        {

        }
    }
}
