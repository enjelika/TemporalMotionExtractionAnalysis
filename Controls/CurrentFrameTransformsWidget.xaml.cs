using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for CurrentFrameTransformsWidget.xaml
    /// </summary>
    public partial class CurrentFrameTransformsWidget : UserControl
    {
        public CurrentFrameTransformsWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(CurrentFrameTransformsWidget), new PropertyMetadata(null));
    }
}
