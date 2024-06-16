using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace TemporalMotionExtractionAnalysis.Behaviors
{
    public class ScrollSynchronizer : Behavior<ListBox>
    {
        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register("CurrentIndex", typeof(int), typeof(ScrollSynchronizer), new PropertyMetadata(0, OnCurrentIndexChanged));

        public int CurrentIndex
        {
            get { return (int)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ScrollSynchronizer)d;
            behavior.ScrollToCurrentIndex();
        }

        private void ScrollToCurrentIndex()
        {
            if (AssociatedObject != null && AssociatedObject.Items.Count > 0)
            {
                var container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(CurrentIndex) as FrameworkElement;
                if (container != null)
                {
                    var scrollViewer = FindVisualChild<ScrollViewer>(AssociatedObject);
                    if (scrollViewer != null)
                    {
                        double itemWidth = container.ActualWidth;
                        double centerOffset = (scrollViewer.ViewportWidth - itemWidth) / 2;

                        scrollViewer.ScrollToHorizontalOffset(CurrentIndex * itemWidth - centerOffset);
                    }
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}
