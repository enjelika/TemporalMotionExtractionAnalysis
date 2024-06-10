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
            var item = value as ListBoxItem;
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(item);
            if (itemsControl != null)
            {
                var index = itemsControl.ItemContainerGenerator.IndexFromContainer(item);
                return index + 1; // Add 1 to start from 1 instead of 0
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
