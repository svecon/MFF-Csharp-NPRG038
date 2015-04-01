using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sverge.DiffWindow;

namespace Sverge
{
    class DiffWindowLoader
    {

        public IDiffWindow CreateWindowFor(object structure)
        {

            if (TextDiff2Way.CanBeApplied(structure))
            {
                return new TextDiff2Way(structure);
            }

            if (TextDiffThreeWay.CanBeApplied(structure))
            {
                return new TextDiffThreeWay(structure);
            }

            throw new ArgumentException("This instance does not have a diff window associated.");
        }

    }
}
