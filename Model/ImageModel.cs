using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace TemporalMotionExtractionAnalysis.Models
{
    public class ImageModel : INotifyPropertyChanged
    {
        private string _imagePath;
        private int _frameNumber;
        private bool _isCurrent;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public int FrameNumber
        {
            get => _frameNumber;
            set
            {
                _frameNumber = value;
                OnPropertyChanged();
            }
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                _isCurrent = value;
                OnPropertyChanged(nameof(IsCurrent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}