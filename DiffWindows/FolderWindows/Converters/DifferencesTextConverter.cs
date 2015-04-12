using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using CoreLibrary.Enums;

namespace DiffWindows.FolderWindows.Converters
{
    class DifferencesTextConverter : MarkupExtension, IMultiValueConverter
    {
        public DifferencesTextConverter() { }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 2)
                throw new ArgumentException("Wrong argument number.");

            if (value[1].GetType() != typeof(DifferencesStatusEnum))
                throw new ArgumentException("Second param is not DifferencesStatusEnum");

            if (value[0].GetType() != typeof(int))
                throw new ArgumentException("First param is not int.");

            var differences = (DifferencesStatusEnum)value[1];

            var location = (LocationEnum)value[0];

            switch ((LocationCombinationsEnum)location)
            {
                case LocationCombinationsEnum.OnLocal:
                    return "Remote file missing";
                case LocationCombinationsEnum.OnRemote:
                    return "Local file missing";
            }

            switch (differences)
            {
                case DifferencesStatusEnum.AllDifferent:
                    return "Files differ";
                //case DifferencesStatusEnum.BaseLocalSame:
                //    return "Remote different";
                //case DifferencesStatusEnum.BaseRemoteSame:
                //    return "Local different";
                //case DifferencesStatusEnum.LocalRemoteSame:
                //    return "Base different";
            }

            return "";
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
