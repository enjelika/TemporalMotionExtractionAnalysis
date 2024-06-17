using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace TemporalMotionExtractionAnalysis.Converters
{
    public class FrameIndexTickVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int frameNumber)
            {
                // Check if the frame number is not a multiple of 6 and is not the current frame
                if (frameNumber % 6 != 0 && !IsCurrentFrame(frameNumber))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        // Add your logic to check if the frame is the current frame
        private bool IsCurrentFrame(int frameNumber)
        {
            // Add your logic here to check if frameNumber is the current frame
            // For example:
            // return frameNumber == ViewModel.CurrentFrameNumber;
            return false; // Placeholder, replace with actual logic
        }
    }
}
