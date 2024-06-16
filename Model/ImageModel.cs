﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TemporalMotionExtractionAnalysis.Model
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}