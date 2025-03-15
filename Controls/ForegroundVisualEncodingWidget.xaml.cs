using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for ForegroundVisualEncodingWidget.xaml
    /// </summary>
    public partial class ForegroundVisualEncodingWidget : UserControl
    {
        public ForegroundVisualEncodingWidget()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(ForegroundVisualEncodingWidget), new PropertyMetadata(null));
    }
}
