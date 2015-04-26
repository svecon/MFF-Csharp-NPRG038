using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreLibrary.Plugins.DiffWindow
{
    public interface IDiffWindowMenu
    {
        MenuItem CreateMenuItem();

        IEnumerable<CommandBinding> CommandBindings();
    }
}
