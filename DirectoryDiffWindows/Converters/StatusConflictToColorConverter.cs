using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CoreLibrary.FilesystemTree.Enums;

namespace DirectoryDiffWindows.Converters
{
    /// <summary>
    /// Converter for marking resolved and conflicting files using two colors.
    /// </summary>
    class StatusConflictToColorConverter : MarkupExtension, IMultiValueConverter
    {
        public StatusConflictToColorConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 3)
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[0].GetType() != typeof(NodeStatusEnum))
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[1].GetType() != typeof(PreferedActionThreeWayEnum))
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[2].GetType() != typeof(int))
                return new SolidColorBrush { Color = Colors.Gray };

            var action = (PreferedActionThreeWayEnum)value[1];
            var status = (NodeStatusEnum)value[0];
            int i = (int)value[2];

            switch (action)
            {
                case PreferedActionThreeWayEnum.ApplyLocal:
                    if (i == 1)
                        return Brushes.DarkGreen;
                    break;
                case PreferedActionThreeWayEnum.RevertToBase:
                    if (i == 2)
                        return Brushes.DarkGreen;
                    break;
                case PreferedActionThreeWayEnum.ApplyRemote:
                    if (i == 3)
                        return Brushes.DarkGreen;
                    break;
            }

            if (status == NodeStatusEnum.IsConflicting)
                return Brushes.Red;

            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
}
