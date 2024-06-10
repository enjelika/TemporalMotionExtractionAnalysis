using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using TemporalMotionExtractionAnalysis.Models;
using System.IO;

namespace TemporalMotionExtractionAnalysis.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ImageModel> _images;
        private ImageModel _currentImage;
        private int _currentIndex;
        private bool _isAnimating;

        public ObservableCollection<ImageModel> Images
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChanged();
            }
        }

        public ImageModel CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadImagesCommand { get; }
        public ICommand StartAnimationCommand { get; }
        public ICommand StopAnimationCommand { get; }

        public MainViewModel()
        {
            Images = new ObservableCollection<ImageModel>();
            LoadImagesCommand = new RelayCommand(LoadImages);
            StartAnimationCommand = new RelayCommand(StartAnimation);
            StopAnimationCommand = new RelayCommand(StopAnimation);
        }

        private void LoadImages()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedPath = dialog.SelectedPath;
                    var imageFiles = Directory.GetFiles(selectedPath, "*.*")
                                              .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                          f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                          f.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

                    Images.Clear();
                    foreach (var file in imageFiles)
                    {
                        Images.Add(new ImageModel { ImagePath = file });
                    }

                    if (Images.Any())
                    {
                        CurrentImage = Images.First();
                        _currentIndex = 0;
                    }
                }
            }
        }


        private async void StartAnimation()
        {
            _isAnimating = true;

            while (_isAnimating)
            {
                await Task.Delay(250); // Adjust delay as needed

                if (!_isAnimating)
                    break;

                _currentIndex = (_currentIndex + 1) % Images.Count;
                CurrentImage = Images[_currentIndex];
            }
        }

        private void StopAnimation()
        {
            _isAnimating = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
