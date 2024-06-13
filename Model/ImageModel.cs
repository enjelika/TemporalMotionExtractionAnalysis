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
        private string _modifiedPicturePath;

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

        public string ModifiedPicturePath
        {
            get => _modifiedPicturePath;
            set
            {
                _modifiedPicturePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}