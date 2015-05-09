using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;

namespace DirectoryDiffWindows.Converters
{
    /// <summary>
    /// Converter printing out time stamp as a readable date.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    class TimestampConverter : MarkupExtension, IValueConverter
    {
        public TimestampConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as FileInfo;

            if (v == null || !v.Exists)
                return "";

            return v.LastWriteTime.ToString("u");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
