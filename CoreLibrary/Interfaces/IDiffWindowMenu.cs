using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreLibrary.Interfaces
{
    public interface IDiffWindowMenu
    {
        MenuItem CreateMenuItem();

        IEnumerable<CommandBinding> CommandBindings();
    }
}
