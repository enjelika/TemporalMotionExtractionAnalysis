using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for PlaybackControlsWidget.xaml
    /// </summary>
    public partial class PlaybackControlsWidget : UserControl
    {
        public PlaybackControlsWidget()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(PlaybackControlsWidget), new PropertyMetadata(null));
    }
}
