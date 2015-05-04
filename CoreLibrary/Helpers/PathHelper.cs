﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoreLibrary.Helpers
{
    public static class PathHelper
    {
        public static string TrimPath(string path, Label filePathLabel, string ellipsis = "...")
        {
            double width = filePathLabel.ActualWidth - 30;

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            if (directory == null)
                return filename;

            bool stringFits;
            bool widthChanged = false;

            Typeface tf = filePathLabel.FontFamily.GetTypefaces().FirstOrDefault() ?? new Typeface("Consolas");

            do
            {
                var formatted = new FormattedText(
                    String.Format("{0}{1}{2}{3}", directory, ellipsis, Path.DirectorySeparatorChar, filename),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    tf,
                    filePathLabel.FontSize,
                    Brushes.Black
                    );

                stringFits = formatted.Width < width;

                if (stringFits)
                {
                    continue;
                }

                widthChanged = true;
                directory = directory.Substring(0, directory.Length - 1);

                if (directory.Length == 0)
                {
                    return string.Format("{0}{1}{2}", ellipsis, Path.DirectorySeparatorChar, filename);
                }

            } while (!stringFits);

            return !widthChanged
                ? path
                : String.Format("{0}{1}{2}{3}", directory, ellipsis, Path.DirectorySeparatorChar, filename);
        }
    }
}
