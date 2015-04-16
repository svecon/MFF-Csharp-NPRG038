using System.Threading.Tasks;
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
        private readonly IDiffWindowManager manager;
        public IFilesystemTreeVisitable DiffNode { get; private set; }

        public DefaultDiffWindow(IFilesystemTreeVisitable instance, IDiffWindowManager manager)
        {
            this.manager = manager;
            DiffNode = instance;

            InitializeComponent();
        }

        public static bool CanBeApplied(object instance)
        {
            return true;
        }

        public void OnDiffComplete(Task t)
        {
        }

        public void OnMergeComplete(Task t)
        {
        }
    }
}
