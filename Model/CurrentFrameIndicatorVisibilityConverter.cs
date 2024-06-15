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
    public class CurrentFrameIndicatorVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming value is an ImageModel instance
            if (value is ImageModel imageModel && imageModel.IsCurrent)
            {
                return Visibility.Visible; // Show the indicator for the current frame
            }

            return Visibility.Collapsed; // Hide the indicator for other frames
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        // Add a property to hold the current frame number (replace 'currentFrameNumber' with your actual property)
        public int CurrentFrameNumber { get; set; }
    }
}
