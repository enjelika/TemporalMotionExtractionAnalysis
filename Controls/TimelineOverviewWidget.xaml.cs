using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.Model;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for TimelineOverviewWidget.xaml
    /// </summary>
    public partial class TimelineOverviewWidget : UserControl
    {
        public TimelineOverviewWidget()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(TimelineOverviewWidget), new PropertyMetadata(null));

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