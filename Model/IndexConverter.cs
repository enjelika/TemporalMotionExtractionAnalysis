using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;

namespace TemporalMotionExtractionAnalysis.Model
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as ComboBoxItem;
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(item);
            if (itemsControl != null)
            {
                var index = itemsControl.ItemContainerGenerator.IndexFromContainer(item);
                return index; // Start from 0
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (int.TryParse(stringValue, out int result))
                {
                    return result; // Successfully parsed the string to an integer
                }
                else
                {
                    // Display error message or log the parsing error
                    Console.WriteLine("Error: Invalid integer format");
                    return -1; // Default value indicating parsing error
                }
            }

            // Return a default value when the string is empty or null
            return -1; // Default value indicating empty string
        }
    }
}
