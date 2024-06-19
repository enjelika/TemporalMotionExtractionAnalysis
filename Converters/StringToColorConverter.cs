﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TemporalMotionExtractionAnalysis.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                if (value.Equals(System.Windows.Media.Colors.DarkGray))
                    return "none";
                else
                    return ((Color)value).ToString();
            }
            return "0"; // Default value in case of an error
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                System.Windows.Media.Color color;

                if (value.Equals("none"))
                {
                    color = System.Windows.Media.Colors.DarkGray; // Transparent;
                }
                else
                {
                   color  = (Color)System.Windows.Media.ColorConverter.ConvertFromString(value.ToString());
                }
                return color;
            }
            return 0; // Default value in case of an error
        }
    }
}
