using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;

namespace BasicMenus.ChangesMenu
{
    /// <summary>
    /// An interface that <see cref="IDiffWindow{TNode}"/> must implement 
    /// in order to handle the <see cref="ChangesMenu"/>
    /// </summary>
    public interface IChangesMenu
    {
        /// <summary>
        /// Action for finding previous difference.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding PreviousCommandBinding(ICommand command);

        /// <summary>
        /// Action for finding next difference.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding NextCommandBinding(ICommand command);

        /// <summary>
        /// Action for relculating diff.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding RecalculateCommandBinding(ICommand command);
    }
}