using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for CompositeViewWidget.xaml
    /// </summary>
    public partial class CompositeViewWidget : UserControl
    {
        public CompositeViewWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(CompositeViewWidget), new PropertyMetadata(null));
    }
}
