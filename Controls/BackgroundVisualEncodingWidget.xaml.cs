
using System.Windows;
using System.Windows.Controls;
using TemporalMotionExtractionAnalysis.ViewModel;

namespace TemporalMotionExtractionAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for BackgroundVisualEncodingWidget.xaml
    /// </summary>
    public partial class BackgroundVisualEncodingWidget : UserControl
    {
        public BackgroundVisualEncodingWidget()
        {
            InitializeComponent();
        }
                public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(BackgroundVisualEncodingWidget), new PropertyMetadata(null));
    }
}
