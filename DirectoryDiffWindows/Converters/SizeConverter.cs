﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;

namespace DirectoryDiffWindows.Converters
{
    /// <summary>
    /// Converting file size to size in kB.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    class SizeConverter : MarkupExtension, IValueConverter
    {
        public SizeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as FileInfo;

            if (v == null || !v.Exists)
                return string.Empty;

            return String.Format("{0}kB", Math.Ceiling(v.Length / 1024.0));
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
