using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;

namespace BasicMenus.MergeMenu
{
    /// <summary>
    /// An interface that <see cref="IDiffWindow{TNode}"/> must implement 
    /// in order to handle the <see cref="MergeMenu"/>
    /// </summary>
    public interface IMergeMenu
    {
        /// <summary>
        /// Action for finding previous conflict.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding PreviousConflictCommandBinding(ICommand command);

        /// <summary>
        /// Action for finding next conflict.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding NextConflictCommandBinding(ICommand command);


        /// <summary>
        /// Action for merging the changes.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding MergeCommandBinding(ICommand command);

        /// <summary>
        /// Action for resolving change by using local version.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding UseLocalCommandBinding(ICommand command);

        /// <summary>
        /// Action for resolving change by using base version.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding UseBaseCommandBinding(ICommand command);

        /// <summary>
        /// Action for resolving change by using remote version.
        /// </summary>
        /// <param name="command">Command associated with this action.</param>
        /// <returns>Command binding for the given action.</returns>
        CommandBinding UseRemoteCommandBinding(ICommand command);
    }
}