﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreLibrary.Interfaces;
using Microsoft.Win32;
using f = System.Windows.Forms;

namespace Sverge
{
    /// <summary>
    /// Interaction logic for OpenDialogWindow.xaml
    /// </summary>
    public partial class OpenDialogWindow : Window, IDataErrorInfo
    {

        public static readonly DependencyProperty LocalLocationProperty = DependencyProperty.Register("LocalLocation", typeof(string), typeof(OpenDialogWindow));

        public string LocalLocation
        {
            get
            {
                return (string)GetValue(LocalLocationProperty);
            }
            set { SetValue(LocalLocationProperty, value); }
        }

        public static readonly DependencyProperty BaseLocationProperty = DependencyProperty.Register("BaseLocation", typeof(string), typeof(OpenDialogWindow));

        public string BaseLocation
        {
            get { return (string)GetValue(BaseLocationProperty); }
            set { SetValue(BaseLocationProperty, value); }
        }


        public static readonly DependencyProperty RemoteLocationProperty = DependencyProperty.Register("RemoteLocation", typeof(string), typeof(OpenDialogWindow));

        public string RemoteLocation
        {
            get { return (string)GetValue(RemoteLocationProperty); }
            set { SetValue(RemoteLocationProperty, value); }
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


        private string currentDirectory;

        private readonly IWindow window;

        public OpenDialogWindow(IWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

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
                window.OpenNewTab(LocalLocation, RemoteLocation);
            } else
            {
                window.OpenNewTab(LocalLocation, BaseLocation, RemoteLocation);
            }

            Close();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}