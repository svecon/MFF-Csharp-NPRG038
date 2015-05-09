using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Plugins.DiffWindow;
using Microsoft.Win32;
using f = System.Windows.Forms;

namespace Sverge
{
    /// <summary>
    /// Interaction logic for OpenDialogWindow.xaml
    /// </summary>
    public partial class OpenDialogWindow : Window, IDataErrorInfo
    {
        #region Dependency properties

        /// <summary>
        /// Dependency property for <see cref="LocalLocation"/>
        /// </summary>
        public static readonly DependencyProperty LocalLocationProperty
            = DependencyProperty.Register("LocalLocation", typeof(string), typeof(OpenDialogWindow));

        /// <summary>
        /// Path to the local file or directory.
        /// </summary>
        public string LocalLocation
        {
            get { return (string)GetValue(LocalLocationProperty); }
            set { SetValue(LocalLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="BaseLocation"/>
        /// </summary>
        public static readonly DependencyProperty BaseLocationProperty
            = DependencyProperty.Register("BaseLocation", typeof(string), typeof(OpenDialogWindow));

        /// <summary>
        /// Path to the base file or directory.
        /// </summary>
        public string BaseLocation
        {
            get { return (string)GetValue(BaseLocationProperty); }
            set { SetValue(BaseLocationProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="RemoteLocation"/>
        /// </summary>
        public static readonly DependencyProperty RemoteLocationProperty
            = DependencyProperty.Register("RemoteLocation", typeof(string), typeof(OpenDialogWindow));

        /// <summary>
        /// Path to the remote file or directory.
        /// </summary>
        public string RemoteLocation
        {
            get { return (string)GetValue(RemoteLocationProperty); }
            set { SetValue(RemoteLocationProperty, value); }
        }

        #endregion

        private string currentDirectory;

        private readonly IDiffWindowManager manager;

        /// <summary>
        /// Initializes new node of the <see cref="OpenDialogWindow"/>
        /// </summary>
        /// <param name="manager">Window manager</param>
        public OpenDialogWindow(IDiffWindowManager manager)
        {
            this.manager = manager;
            InitializeComponent();
        }

        public string Error
        {
            get { return this[null]; }
        }

        public string this[string columnName]
        {
            get
            {
                string path = "";
                int errorPosition = 0;
                switch (columnName)
                {
                    case "LocalLocation":
                        path = LocalLocation;
                        errorPosition = 1;
                        break;
                    case "BaseLocation":
                        path = BaseLocation;
                        errorPosition = 2;
                        break;
                    case "RemoteLocation":
                        path = RemoteLocation;
                        errorPosition = 4;
                        break;
                }

                if (columnName == "BaseLocation" && string.IsNullOrEmpty(path))
                {
                    errors &= ~errorPosition;
                    return null;
                }

                if (File.Exists(path) || Directory.Exists(path))
                {
                    errors &= ~errorPosition;
                    return null;
                }

                errors |= errorPosition;
                return columnName;
            }
        }

        private int errors;

        private TextBox GetTextBox(object tag)
        {
            switch ((string)tag)
            {
                case "local":
                    return LocalPathBox;
                case "base":
                    return BasePathBox;
                case "remote":
                    return RemotePathBox;
            }

            throw new ArgumentException("No TextBox found for this tag.");
        }

        private void FileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.FileOk += (o, args) =>
            {
                var fileDialog = o as OpenFileDialog;

                if (fileDialog == null)
                    return;

                GetTextBox(((Button)sender).Tag).Text = fileDialog.FileName;
            };

            dialog.ShowDialog();
        }

        private void DirectoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new f.FolderBrowserDialog();

            if (currentDirectory != null)
            {
                dialog.SelectedPath = currentDirectory;
            }

            f.DialogResult result = dialog.ShowDialog();

            if (result != f.DialogResult.OK)
                return;

            GetTextBox(((Button)sender).Tag).Text = dialog.SelectedPath;
            currentDirectory = dialog.SelectedPath;
        }

        /// <summary>
        /// Command for opening new visualisation window.
        /// </summary>
        public static readonly RoutedUICommand OpenNewWindow = new RoutedUICommand(
            "OpenNewWindow", "OpenNewWindow",
            typeof(OpenDialogWindow),
            new InputGestureCollection() { 
                new KeyGesture(Key.Enter)
            }
        );

        private void OpenNewWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = errors == 0;
        }

        private void OpenNewWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(BaseLocation))
            {
                manager.OpenNewTab(LocalLocation, RemoteLocation);
            } else
            {
                manager.OpenNewTab(LocalLocation, BaseLocation, RemoteLocation);
            }

            Close();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
