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
using TemporalMotionExtractionAnalysis.Model;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;

namespace TemporalMotionExtractionAnalysis.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ImageModel> _images;
        private int _imageCount;
        private ImageModel _currentImage;
        private int _currentIndex;
        private bool _isAnimating;
        private string _folderName;
        private ObservableCollection<ImageModel> _selectedFrames;

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

        public int ImageCount
        {
            get { return _imageCount; }
            set
            {
                _imageCount = value;
                OnPropertyChanged(nameof(ImageCount));
            }
        }

        public string FolderName
        {
            get => _folderName;
            set
            {
                _folderName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ImageModel> SelectedFrames
        {
            get => _selectedFrames;
            set
            {
                _selectedFrames = value;
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

            FolderName = "No Selected Folder";
            SelectedFrames = new ObservableCollection<ImageModel>();

            StartMotionExtraction();
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

                    ImageCount = imageFiles.Count(); // Update image count

                    Images.Clear();
                    int frameNumber = 1;
                    foreach (var file in imageFiles)
                    {
                        Images.Add(new ImageModel { ImagePath = file, FrameNumber = frameNumber++ });
                    }

                    if (Images.Any())
                    {
                        CurrentImage = Images.First();
                        _currentIndex = 0;
                    }

                    FolderName = System.IO.Path.GetFileName(selectedPath);
                }
            }
        }

        private void StartMotionExtraction()
        {
            //Uri path1 = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\MoCA\\JPEGImages\\arabian_horn_viper\\00000.jpg");
            //Mat prev_mask = Cv2.ImRead(path1.AbsolutePath);
            //Uri path2 = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\MoCA\\JPEGImages\\arabian_horn_viper\\00001.jpg");
            //Mat curr_mask = Cv2.ImRead(path2.AbsolutePath);
            //MotionExtraction.calculate_e_measure_pixelwise(prev_mask, curr_mask, 10);
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

        public void HandleSelectionChanged(System.Collections.IList selectedItems)
        {
            if (selectedItems.Count <= 2)
            {
                SelectedFrames.Clear();
                foreach (var item in selectedItems)
                {
                    if (item is ImageModel imageModel)
                    {
                        SelectedFrames.Add(imageModel);
                    }
                }
            }
            else
            {
                // Handle the case where more than two items are selected (optional)
                // For example, you can show a message to the user or automatically deselect items
            }
        }
    }
}
