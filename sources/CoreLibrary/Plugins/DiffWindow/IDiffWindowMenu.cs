using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreLibrary.Plugins.DiffWindow
{
    /// <summary>
    /// Interface for a menu that is used to call actions on the <see cref="IDiffWindow{TNode}"/>
    /// </summary>
    public interface IDiffWindowMenu
    {
        /// <summary>
        /// Creates a menu item that is then insterted into main menu
        /// </summary>
        /// <returns>Menu item containing all available actions.</returns>
        MenuItem CreateMenuItem();

        /// <summary>
        /// Command bindings are passed to higher structure that can listen on them.
        /// </summary>
        /// <returns>Returns all command bidings that are used in the menu.</returns>
        IEnumerable<CommandBinding> CommandBindings();
    }
}
