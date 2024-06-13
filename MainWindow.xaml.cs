using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TemporalMotionExtractionAnalysis.Models;
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

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                var imageModel = image.DataContext as ImageModel;
                if (imageModel != null)
                {
                    // imageModel.StartAnimation(image); // This line can be removed if not used
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.HandleSelectionChanged(listBox.SelectedItems);
                }
            }
        }
    }
}