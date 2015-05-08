using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using CoreLibrary.FilesystemTree.Enums;

namespace DirectoryDiffWindows.Converters
{
    class DifferencesTextConverter : MarkupExtension, IMultiValueConverter
    {
        public DifferencesTextConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 2)
                return string.Empty;

            if (value[1] == DependencyProperty.UnsetValue || value[1].GetType() != typeof(DifferencesStatusEnum))
                return string.Empty;

            if (value[0] == DependencyProperty.UnsetValue || value[0].GetType() != typeof(int))
                return string.Empty;

            var differences = (DifferencesStatusEnum)value[1];

            var location = (LocationEnum)value[0];

            switch ((LocationCombinationsEnum)location)
            {
                case LocationCombinationsEnum.OnLocal:
                    return Resources.Differences_RemoteMissing;
                case LocationCombinationsEnum.OnRemote:
                    return Resources.Differences_LocalMissing;
            }

            switch (differences)
            {
                case DifferencesStatusEnum.AllDifferent:
                    return Resources.Differences_FilesDifferent;
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
}
