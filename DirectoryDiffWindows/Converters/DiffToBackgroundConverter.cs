using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CoreLibrary.Enums;

namespace DirectoryDiffWindows.Converters
{
    class DiffToBackgroundConverter : MarkupExtension, IMultiValueConverter
    {
        public DiffToBackgroundConverter() { }

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

            switch ((LocationCombinationsEnum)location)
            {
                case LocationCombinationsEnum.OnLocal:
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.PaleVioletRed, 1));
                    return gradientBrush;

                case LocationCombinationsEnum.OnRemote:
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.PaleVioletRed, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));
                    return gradientBrush;
            }

            switch (differences)
            {
                case DifferencesStatusEnum.AllDifferent:
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));

                    return new SolidColorBrush() { Color = Colors.Purple, Opacity = .4};
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
