using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for FolderLoaderWidget.xaml
    /// </summary>
    public partial class FolderLoaderWidget : UserControl
    {
        public FolderLoaderWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(FolderLoaderWidget), new PropertyMetadata(null));
    }
}
