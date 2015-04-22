﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CoreLibrary.Enums;

namespace DiffWindows.FolderWindows.Converters
{
    class StatusConflictToColorConverter : MarkupExtension, IMultiValueConverter
    {
        public StatusConflictToColorConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 3)
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[0].GetType() != typeof(NodeStatusEnum))
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[1].GetType() != typeof(PreferedActionEnum))
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[2].GetType() != typeof(int))
                return new SolidColorBrush { Color = Colors.Gray };

            var action = (PreferedActionEnum)value[1];
            var status = (NodeStatusEnum)value[0];
            int i = (int)value[2];

            switch (action)
            {
                case PreferedActionEnum.ApplyLocal:
                    if (i == 1)
                        return Brushes.DarkGreen;
                    break;
                case PreferedActionEnum.RevertToBase:
                    if (i == 2)
                        return Brushes.DarkGreen;
                    break;
                case PreferedActionEnum.ApplyRemote:
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