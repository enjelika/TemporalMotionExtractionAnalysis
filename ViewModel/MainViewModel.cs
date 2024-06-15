using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Forms;
using TemporalMotionExtractionAnalysis.Models;
using System.IO;
using TemporalMotionExtractionAnalysis.Model;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using System.Diagnostics;

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
        private int _offsetValue;
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

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                OnPropertyChanged();
            }
        }

        public int OffsetValue
        {
            get => _offsetValue;
            set
            {
                _offsetValue = value;
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
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand StartMotionExtractionCommand { get; }

        public MainViewModel()
        {
            OffsetValue = 0;
            Images = new ObservableCollection<ImageModel>();
            LoadImagesCommand = new RelayCommand(LoadImages);
            PlayCommand = new RelayCommand(OnPlay);
            PauseCommand = new RelayCommand(OnStop);
            StopCommand = new RelayCommand(OnStop);
            PreviousCommand = new RelayCommand(OnPrevious);
            NextCommand = new RelayCommand(OnNext);
            StartMotionExtractionCommand = new RelayCommand(StartMotionExtraction);

            FolderName = "No Selected Folder";
            SelectedFrames = new ObservableCollection<ImageModel>();
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
                        CurrentIndex = 0;
                    }

                    FolderName = System.IO.Path.GetFileName(selectedPath);
                }
            }
        }

        private void StartMotionExtraction()
        {
            //Uri path1 = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\MoCA\\JPEGImages\\arabian_horn_viper\\00000.jpg");
            //Mat prev_mask = Cv2.ImRead(path1.AbsolutePath, ImreadModes.Grayscale);
            //Uri path2 = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\MoCA\\JPEGImages\\arabian_horn_viper\\00001.jpg");
            //Mat curr_mask = Cv2.ImRead(path2.AbsolutePath, ImreadModes.Grayscale);
            //MotionExtraction.calculate_ssim(prev_mask, curr_mask);
        }

        private async void OnPlay()
        {
            _isAnimating = true;

            while (_isAnimating)
            {
                await Task.Delay(250); // Adjust delay as needed

                if (!_isAnimating)
                    break;

                CurrentIndex = (CurrentIndex + 1) % Images.Count;
                CurrentImage = Images[CurrentIndex];
            }
        }

        private void OnStop()
        {
            _isAnimating = false;
        }

        private void OnPrevious()
        {
            if (Images.Count == 0)
                return;

            _isAnimating = false;
            CurrentIndex = (CurrentIndex - 1 + Images.Count) % Images.Count;
            CurrentImage = Images[CurrentIndex];
        }

        private void OnNext()
        {
            if (Images.Count == 0)
                return;

            _isAnimating = false;
            CurrentIndex = (CurrentIndex + 1) % Images.Count;
            CurrentImage = Images[CurrentIndex];
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="name">The name of the property that changed, which is optional and provided automatically by the CallerMemberName attribute.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Handles the selection change event for a collection of selected items.
        /// Clears and updates the SelectedFrames collection with new selected items,
        /// ensuring that no more than two items are selected at a time.
        /// </summary>
        /// <param name="selectedItems">The collection of selected items.</param>
        /// <remarks>
        /// If the number of selected items is greater than two, this method can handle
        /// the situation by either showing a message to the user or automatically
        /// deselecting items (this part is currently optional and can be customized).
        /// </remarks>
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
                // Log an error
                Debug.WriteLine("Error: More than two items were selected.");
            }
        }
    }
}
