using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;

namespace Sverge
{
    /// <summary>
    /// A window for showing all processors and their settings.
    /// 
    /// Also allows to change the settings.
    /// </summary>
    public partial class ProcessorSettingsWindow : Window
    {
        /// <summary>
        /// Loader containing all avaialble processors.
        /// </summary>
        private readonly IProcessorLoader loader;

        /// <summary>
        /// Dictionary for settings and their inputs.
        /// </summary>
        private readonly Dictionary<ISettings, Control> controls;

        /// <summary>
        /// Initializes new node of the <see cref="ProcessorSettingsWindow"/>
        /// </summary>
        /// <param name="processorLoader">Processor loader containing all loaded processors.</param>
        public ProcessorSettingsWindow(IProcessorLoader processorLoader)
        {
            loader = processorLoader;
            controls = new Dictionary<ISettings, Control>();

            InitializeComponent();
            CreateForm();
        }

        /// <summary>
        /// Creates form for the available settings.
        /// 
        /// Different inputs for different types of settings.
        /// </summary>
        private void CreateForm()
        {
            foreach (IProcessor processor in loader.GetProcessors(ProcessorTypeEnum.Diff)
                                     .Concat(loader.GetProcessors(ProcessorTypeEnum.Merge)))
            {
                int i = 0;

                var attr = (ProcessorAttribute)processor.GetType().GetCustomAttribute(typeof(ProcessorAttribute));

                GroupBox box;
                if (attr == null)
                {
                    box = new GroupBox { Header = processor.GetType().Name };
                } else
                {
                    box = new GroupBox {
                        Header = string.Format("{0}: {1} [{2}: {3}]",
                            attr.Priority,
                            processor.GetType().Name,
                            attr.ProcessorType,
                            attr.Mode
                            )
                    };
                }

                var boxContent = new Grid();
                boxContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
                boxContent.ColumnDefinitions.Add(new ColumnDefinition());

                if (!loader.GetSettingsByProcessor(processor).Any())
                {
                    boxContent.RowDefinitions.Add(new RowDefinition());

                    var settingsLabel = new Label {
                        Content = Properties.Resources.ProcessorSettings_NoSettingsFound
                    };

                    boxContent.Children.Add(settingsLabel);
                    Grid.SetRow(settingsLabel, i);
                    Grid.SetColumn(settingsLabel, 0);
                    Grid.SetColumnSpan(settingsLabel, 2);
                }

                foreach (ISettings settings in loader.GetSettingsByProcessor(processor))
                {
                    boxContent.RowDefinitions.Add(new RowDefinition());

                    var settingsLabel = new Label { Content = settings.Info };

                    boxContent.Children.Add(settingsLabel);
                    Grid.SetRow(settingsLabel, i);
                    Grid.SetColumn(settingsLabel, 1);

                    Control control = CreateInput(settings);

                    boxContent.Children.Add(control);
                    Grid.SetRow(control, i);
                    Grid.SetColumn(control, 0);

                    i++;
                }

                box.Content = boxContent;
                Container.Children.Add(box);
            }

            var saveButton = new Button { Content = Properties.Resources.ProcessorSettings_Save };
            saveButton.Click += (sender, args) => SaveValues();

            Container.Children.Add(saveButton);
        }

        /// <summary>
        /// Creates an input box depending on Setting's type.
        /// </summary>
        /// <param name="settings">Setting for which the input box will be created.</param>
        /// <returns>Control</returns>
        private Control CreateInput(ISettings settings)
        {
            object value = settings.GetValue();

            Control newControl;
            if (value is bool)
            {
                newControl = new CheckBox { IsChecked = (bool)value };
            } else if (value is Enum)
            {
                newControl = new ComboBox { ItemsSource = Enum.GetValues(value.GetType()), SelectedValue = value };
            } else if (value == null)
            {
                newControl = new TextBox { Text = string.Empty };
            } else if (value.GetType().IsArray)
            {
                newControl = new TextBox { Text = ArrayToString(value) };
            } else
            {
                newControl = new TextBox { Text = value.ToString() };
            }

            controls.Add(settings, newControl);
            return newControl;
        }

        /// <summary>
        /// Converts an array to string separated by commas
        /// </summary>
        /// <param name="array">Object which is an instance to an array.</param>
        /// <returns>String of values delimited by commas.</returns>
        private static string ArrayToString(object array)
        {
            var a = array as Array;

            if (a == null)
                return string.Empty;

            var o = new object[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                o[i] = a.GetValue(i);
            }

            return string.Join(",", o);
        }

        /// <summary>
        /// Gets a value from a control.
        /// </summary>
        /// <param name="c">Control</param>
        /// <returns>String value of a control.</returns>
        private static string ControlToValue(Control c)
        {
            var checkBox = c as CheckBox;
            var comboBox = c as ComboBox;
            var textBox = c as TextBox;
            if (checkBox != null)
            {
                return checkBox.IsChecked.ToString();
            } else if (comboBox != null)
            {
                return comboBox.SelectedValue.ToString();
            } else if (textBox != null)
            {
                return textBox.Text;
            } else
            {
                throw new ArgumentException("Unknown control.");
            }
        }

        /// <summary>
        /// Save the current values of the settings.
        /// </summary>
        private void SaveValues()
        {
            foreach (KeyValuePair<ISettings, Control> keyValuePair in controls)
            {
                try
                {
                    string val = ControlToValue(keyValuePair.Value);
                    keyValuePair.Key.SetValue(string.IsNullOrWhiteSpace(val) ? null : val);
                } catch (Exception)
                {
                    // ignore wrong user inputs
                }
            }

            Close();
        }
    }
}
