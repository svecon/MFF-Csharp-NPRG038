using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sverge.DiffWindow
{
    [ValueConversion(typeof(object), typeof(string))]
    class TimeFormatConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = value as FileInfo;

            if (v == null)
                return "";

            return v.LastWriteTime.ToShortDateString();//String.Format("{0}kB", Math.Ceiling(v.Length / 1024.0));
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
