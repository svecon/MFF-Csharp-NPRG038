using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using CoreLibrary.Enums;

namespace Sverge.DiffWindow
{
    [ValueConversion(typeof(object), typeof(string))]
    class DifferencesTextConverter : MarkupExtension, IValueConverter
    {
        public DifferencesTextConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() != typeof(DifferencesStatusEnum))
                return "";

            var differencesStatus = (DifferencesStatusEnum)value;

            switch (differencesStatus)
            {
                case DifferencesStatusEnum.AllDifferent:
                    return "All different";
                case DifferencesStatusEnum.AllSame:
                    return "";
                case DifferencesStatusEnum.BaseLocalSame:
                    return "Remote different";
                case DifferencesStatusEnum.BaseRemoteSame:
                    return "Local different";
                case DifferencesStatusEnum.LocalRemoteSame:
                    return "Base different";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
