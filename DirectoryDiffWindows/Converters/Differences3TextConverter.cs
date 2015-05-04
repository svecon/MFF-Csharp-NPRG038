using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using CoreLibrary.Enums;

namespace DirectoryDiffWindows.Converters
{
    class Differences3TextConverter : MarkupExtension, IMultiValueConverter
    {
        public Differences3TextConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 2)
                return string.Empty;

            if (value[1] == DependencyProperty.UnsetValue || value[1].GetType() != typeof (DifferencesStatusEnum))
                return string.Empty;

            if (value[0] == DependencyProperty.UnsetValue || value[0].GetType() != typeof (int))
                return string.Empty;

            var differences = (DifferencesStatusEnum)value[1];

            var location = (LocationEnum)value[0];

            switch ((LocationCombinationsEnum)location)
            {
                case LocationCombinationsEnum.OnAll3:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            return Resources.Differences_FilesDifferent;
                        case DifferencesStatusEnum.BaseLocalSame:
                            return Resources.Differences_RemoteDifferent;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            return Resources.Differences_LocalDifferent;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            return Resources.Differences_BaseDifferent;
                    }
                    break;
                case LocationCombinationsEnum.OnBase:
                    return Resources.Differences_BaseOnly;
                case LocationCombinationsEnum.OnLocal:
                    return Resources.Differences_LocalOnly;
                case LocationCombinationsEnum.OnRemote:
                    return Resources.Differences_RemoteOnly;
                case LocationCombinationsEnum.OnBaseLocal:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            return Resources.Differences_FilesDifferent;
                        case DifferencesStatusEnum.BaseLocalSame:
                            return Resources.Differences_RemoteDeleted;
                    }
                    break;
                case LocationCombinationsEnum.OnBaseRemote:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            return Resources.Differences_FilesDifferent;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            return Resources.Differences_LocalDeleted;
                    }
                    break;
                case LocationCombinationsEnum.OnLocalRemote:
                    switch (differences)
                    {
                        case DifferencesStatusEnum.AllDifferent:
                            return Resources.Differences_FilesDifferent;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            return Resources.Differences_BaseMissing;
                    }
                    break;
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
