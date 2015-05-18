using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Sverge
{
    /// <summary>
    /// Window containing information about the project: version, author and online sources.
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Initializes new node of the <see cref="AboutWindow"/>
        /// </summary>
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens given URL in the browser.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Navigate event args.</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
