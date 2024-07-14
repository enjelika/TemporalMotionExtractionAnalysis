using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TemporalMotionExtractionAnalysis.Controls;
using TemporalMotionExtractionAnalysis.Model;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SSIMImageControl _ssimImageControl;
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _ssimImageControl = new SSIMImageControl();
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