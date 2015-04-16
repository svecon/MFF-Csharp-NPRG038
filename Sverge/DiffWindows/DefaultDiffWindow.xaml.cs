using System.Windows.Controls;
using CoreLibrary.Interfaces;
using CoreLibrary.Plugins.DiffWindow;

namespace Sverge.DiffWindows
{
    using DW = IDiffWindow<IFilesystemTreeVisitable>;

    /// <summary>
    /// Interaction logic for DefaultDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue)]
    public partial class DefaultDiffWindow : UserControl, DW
    {
        private readonly IWindow window;
        public IFilesystemTreeVisitable DiffNode { get; private set; }

        public DefaultDiffWindow(IFilesystemTreeVisitable instance, IWindow window)
        {
            this.window = window;
            DiffNode = instance;

            InitializeComponent();
        }

        public static bool CanBeApplied(object instance)
        {
            return true;
        }

        public void OnDiffComplete()
        {
        }

        public void OnMergeComplete()
        {
        }
    }
}
