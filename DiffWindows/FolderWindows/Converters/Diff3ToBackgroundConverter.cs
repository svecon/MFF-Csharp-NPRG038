using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CoreLibrary.Enums;

namespace DiffWindows.FolderWindows.Converters
{
    class Diff3ToBackgroundConverter : MarkupExtension, IMultiValueConverter
    {
        public Diff3ToBackgroundConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 2)
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[1].GetType() != typeof(DifferencesStatusEnum))
                return new SolidColorBrush { Color = Colors.Gray };

            if (value[0].GetType() != typeof(int))
                return new SolidColorBrush { Color = Colors.Gray };

            var differences = (DifferencesStatusEnum)value[1];

            var location = (LocationEnum)value[0];

            var gradientBrush = new LinearGradientBrush {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            //switch ((LocationCombinationsEnum)location)
            //{
            //    case LocationCombinationsEnum.OnLocal:
            //        gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0));
            //        gradientBrush.GradientStops.Add(new GradientStop(Colors.PaleVioletRed, 1));
            //        return gradientBrush;

            //    case LocationCombinationsEnum.OnRemote:
            //        gradientBrush.GradientStops.Add(new GradientStop(Colors.PaleVioletRed, 0));
            //        gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));
            //        return gradientBrush;
            //}

            //switch (differences)
            //{
            //    case DifferencesStatusEnum.AllDifferent:
            //        gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0));
            //        gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));

            //}

            Color changes = Color.FromArgb(100, 128, 0, 128);
            Color transparentWhite = Color.FromArgb(0, 255, 255, 255);
            Color deleted = Colors.PaleVioletRed;
            Color created = Colors.LightGreen;

            switch ((LocationCombinationsEnum)location)
            {
                case LocationCombinationsEnum.OnAll3:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            return new SolidColorBrush() { Color = changes };
                        case DifferencesStatusEnum.BaseLocalSame:
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 1));
                            return gradientBrush;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            return gradientBrush;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 1.0 / 2));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1));
                            return gradientBrush;
                    }
                    break;
                case LocationCombinationsEnum.OnBase:
                    gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(deleted, 1.0 / 2));
                    gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1));
                    return gradientBrush;
                case LocationCombinationsEnum.OnLocal:
                    gradientBrush.GradientStops.Add(new GradientStop(created, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                    return gradientBrush;
                case LocationCombinationsEnum.OnRemote:
                    gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                    gradientBrush.GradientStops.Add(new GradientStop(created, 1));
                    return gradientBrush;
                case LocationCombinationsEnum.OnBaseLocal:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            return gradientBrush;
                        case DifferencesStatusEnum.BaseLocalSame:
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            gradientBrush.GradientStops.Add(new GradientStop(deleted, 1));
                            return gradientBrush;
                    }
                    break;
                case LocationCombinationsEnum.OnBaseRemote:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 1.0 / 3));
                            return gradientBrush;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            gradientBrush.GradientStops.Add(new GradientStop(deleted, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            return gradientBrush;
                    }
                    break;
                case LocationCombinationsEnum.OnLocalRemote:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            gradientBrush.GradientStops.Add(new GradientStop(changes, 1));
                            return gradientBrush;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            gradientBrush.GradientStops.Add(new GradientStop(created, 0));
                            gradientBrush.GradientStops.Add(new GradientStop(transparentWhite, 1.0 / 2));
                            gradientBrush.GradientStops.Add(new GradientStop(created, 1));
                            return gradientBrush;
                    }
                    break;
            }

            return "";
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
