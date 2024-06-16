using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace TemporalMotionExtractionAnalysis.Model
{
    public class IndexBasedTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SecondItemTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

            if (index == 1) // Index 1 means the second item (0-based index)
            {
                return SecondItemTemplate;
            }

            return DefaultTemplate;
        }
    }
}
