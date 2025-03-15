using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.Model;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for ZoomedTimelineWidget.xaml
    /// </summary>
    public partial class ZoomedTimelineWidget : UserControl
    {
        public ZoomedTimelineWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(ZoomedTimelineWidget), new PropertyMetadata(null));

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;

            if (listBox != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.HandleSelectionChanged(listBox.SelectedItems);
                }
            }
        }

        private void ListBoxItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var listBoxItem = sender as System.Windows.Controls.ListBoxItem;
            ImageModel image = listBoxItem.DataContext as ImageModel;

            if (image != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.HandleIndicationSelectionChanged(image);
                }
            }
        }
    }
}
