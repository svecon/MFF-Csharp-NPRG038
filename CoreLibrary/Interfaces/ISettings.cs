using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Interfaces
{
    public interface ISettings
    {
        //Type ForType { get; }

        string Info { get; }

        string Option { get; }

        string OptionShortcut { get; }

        bool WasSet { get; }

        int NumberOfParams { get; }

        void SetValue(params string[] value);

        object Instance { get; }

        FieldInfo Field { get; }
    }
}
