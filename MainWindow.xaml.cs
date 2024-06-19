using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

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

        private void currentFrameTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = Convert.ToInt32(e.NewValue);
            string msg = String.Format("Level: {0}", value);
            this.textBlock1.Text = msg;
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.CurrentFrameTransparency = value;
            }
        }

        private void offsetFrameTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = Convert.ToInt32(e.NewValue);
            string msg = String.Format("Level: {0}", value);
            this.textBlock2.Text = msg;
            //var viewModel = DataContext as MainViewModel;
            //if (viewModel != null)
            //{
            //    viewModel.OffsetFrameTransparency = value;
            //}
        }
    }
}