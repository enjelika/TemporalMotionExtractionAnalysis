using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TemporalMotionExtractionAnalysis.Model
{
    public class ImageModel : INotifyPropertyChanged
    {
        private string _imagePath;
        private int _frameNumber;
        private bool _isCurrent;
        private bool _isOffsetSelection;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public int FrameNumber
        {
            get => _frameNumber;
            set
            {
                _frameNumber = value;
                OnPropertyChanged(nameof(FrameNumber));
            }
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                if (_isCurrent != value)
                {
                    _isCurrent = value;
                    OnPropertyChanged(nameof(IsCurrent));
                }
            }
        }

        public bool IsOffsetSelection
        {
            get => _isOffsetSelection;
            set
            {
                _isOffsetSelection = value;
                OnPropertyChanged(nameof(IsOffsetSelection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Clear method to reset the ImageModel properties to default values
        public void Clear()
        {
            ImagePath = string.Empty;
            FrameNumber = 0;
            IsCurrent = false;
            IsOffsetSelection = false;
        }
    }
}