using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using CoreLibrary.Exceptions;
using CoreLibrary.Plugins;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace Sverge
{
    /// <summary>
    /// An entry point for the graphical user interface.
    /// </summary>
    public partial class App : Application
    {
        private IProcessorLoader loader;

        private MainWindow diffManager;

        const int ERROR_PROCESSOR_COLLISION = 9;
        const int ERROR_SETTINGS_NOT_FOUND = 10;
        const int ERROR_SETTINGS_UNKNOWN = 11;

        /// <summary>
        /// Return assembly version for current program
        /// </summary>
        /// <returns>Current assembly version</returns>
        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private void App_OnStartup(object sender, StartupEventArgs eventArgs)
        {
            #region DEBUG: Print arguments
            // TODO: remove this
#if DEBUG
            //string[] args = Environment.GetCommandLineArgs();
            using (TextWriter tw = File.CreateText("C:/Users/svecon/Downloads/git-arguments.txt"))
            {
                foreach (string s in eventArgs.Args)
                {
                    tw.WriteLine(s);
                }

                tw.WriteLine(">>>");
                tw.WriteLine(Environment.CurrentDirectory);
                tw.WriteLine(System.Windows.Forms.Application.ExecutablePath);
                tw.WriteLine(Assembly.GetEntryAssembly().Location);
            }
#endif
            #endregion

            #region Load all available processors and their settings
            try
            {
                PluginsLoader.LoadAssemblies();
                loader = new ProcessorLoader();
                // Load available processors and their settings
                loader.LoadAll();
            } catch (ProcessorPriorityColissionException e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_ProcessorCollision, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_PROCESSOR_COLLISION);
                return;
            }
            #endregion

            #region Parse arguments as settings

            string[] args = eventArgs.Args;
            try
            {
                // pass the arguments and parse them
                var parser = new SettingsParser(loader.GetSettings());
                args = parser.ParseSettings(args);
            } catch (SettingsNotFoundException e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_InvalidSettingsValue, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_SETTINGS_NOT_FOUND);
                return;
            } catch (SettingsUnknownValue e)
            {
                MessageBox.Show(
                    string.Format(Sverge.Properties.Resources.App_OnStartup_UnknownSettingsValue, e.Message),
                    Sverge.Properties.Resources.App_OnStartup_ErrorParsingSettings
                );
                Current.Shutdown(ERROR_SETTINGS_UNKNOWN);
                return;
            }
            #endregion

            diffManager = new MainWindow(loader);
            diffManager.Show();

            if (args.Length > 0)
            {
                diffManager.OpenNewTab(args);
            }
            else
            {
                diffManager.OpenWindowDialog();
            }
        }

        private void App_OnActivated(object sender, EventArgs e)
        {
            // TODO: check and possibly recalculate diff
        }

        private void App_OnDeactivated(object sender, EventArgs e)
        {
        }

        private void App_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
        }
    }
}
