using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for AnimationPaneWidget.xaml
    /// </summary>
    public partial class AnimationPaneWidget : UserControl
    {
        public AnimationPaneWidget()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(AnimationPaneWidget), new PropertyMetadata(null));
    }
}
