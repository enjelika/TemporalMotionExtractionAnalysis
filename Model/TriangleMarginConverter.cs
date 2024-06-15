using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace TemporalMotionExtractionAnalysis.Model
{
    public class TriangleMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is int currentIndex) || !(values[1] is int imageCount))
                return new Thickness(0);

            // Calculate margin based on currentIndex and imageCount
            // Adjust this logic as needed to position the triangle correctly
            double trianglePosition = (double)currentIndex / imageCount * 830; // Example calculation
            return new Thickness(trianglePosition, 0, 0, 0); // Left margin based on percentage
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
