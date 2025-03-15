using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for OffsetFrameTransformsWidget.xaml
    /// </summary>
    public partial class OffsetFrameTransformsWidget : UserControl
    {
        public OffsetFrameTransformsWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(OffsetFrameTransformsWidget), new PropertyMetadata(null));
    }
}
