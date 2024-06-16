using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Forms;
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
        private bool _isReversePlayback;
        private string _folderName;
        private int _offsetValue;
        private int _fpsValue;
        private int _timeDelay;
        private ObservableCollection<ImageModel> _selectedFrames;

        public ObservableCollection<ImageModel> Images
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChanged(nameof(Images));
            }
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex != value)
                {
                    if (_currentIndex >= 0 && _currentIndex < Images.Count)
                    {
                        Images[_currentIndex].IsCurrent = false;
                    }

                    _currentIndex = value;

                    if (_currentIndex >= 0 && _currentIndex < Images.Count)
                    {
                        Images[_currentIndex].IsCurrent = true;
                    }

                    OnPropertyChanged(nameof(CurrentIndex));
                }
            }
        }

        public int OffsetValue
        {
            get => _offsetValue;
            set
            {
                _offsetValue = value;
                Console.WriteLine(OffsetValue.ToString()); // Debugging
                OnPropertyChanged(nameof(OffsetValue));
            }
        }
        public int FpsValue
        {
            get => _fpsValue;
            set
            {
                if (_fpsValue != value)
                {
                    _fpsValue = value;
                    OnPropertyChanged(nameof(FpsValue)); // Notify property changed
                                                         // Update the time delay based on the new FPS value
                    TimeDelay = ConvertFPSToTimeDelay(FpsValue);
                }
            }
        }

        public int TimeDelay
        {
            get => _timeDelay;
            set
            {
                if (_timeDelay != value)
                {
                    _timeDelay = value;
                    OnPropertyChanged(nameof(TimeDelay)); // Notify property changed
                }
            }
        }


        public ImageModel CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        public bool IsReversePlayback
        {
            get => _isReversePlayback;
            set
            {
                _isReversePlayback = value;
                OnPropertyChanged(nameof(IsReversePlayback));
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
                OnPropertyChanged(nameof(FolderName));
            }
        }

        public ObservableCollection<ImageModel> SelectedFrames
        {
            get => _selectedFrames;
            set
            {
                _selectedFrames = value;
                OnPropertyChanged(nameof(SelectedFrames));
            }
        }

        public ICommand LoadImagesCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand StartMotionExtractionCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// Sets up initial state, commands, and collections.
        /// </summary>
        public MainViewModel()
        {
            OffsetValue = 0; // Default value
            TimeDelay = 250; // Default value
            FpsValue = (int)ConvertTimeDelayToFPS(TimeDelay); // Default value is 4
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

        /// <summary>
        /// Converts frames per second (FPS) to a time delay in milliseconds.
        /// </summary>
        /// <param name="fps">The frames per second value to convert.</param>
        /// <returns>The corresponding time delay in milliseconds.</returns>
        public int ConvertFPSToTimeDelay(int fps)
        {
            if (fps < 4) 
            { 
                return (int)(1000.0 / fps); // Convert FPS to time delay in milliseconds
            }
            else
            {
                return 250;
            }
        }

        /// <summary>
        /// Converts a time delay in milliseconds to frames per second (FPS).
        /// </summary>
        /// <param name="timeDelay">The time delay in milliseconds.</param>
        /// <returns>The FPS value calculated from the time delay.</returns>
        /// <exception cref="ArgumentException">Thrown when the time delay is not a positive value greater than zero.</exception>
        public double ConvertTimeDelayToFPS(int timeDelay)
        {
            if (timeDelay <= 250)
            {
                return 1000.0 / timeDelay;
            }
            else // FPS default value is 4
            {
                return 4;
            }
        }

        /// <summary>
        /// Loads image files from a selected folder into the Images collection.
        /// </summary>
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
                    int frameNumber = 0;
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
            //Uri sourcePath = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\blue_triangle.jpg");
            //Mat source = Cv2.ImRead(sourcePath.AbsolutePath, ImreadModes.Color);
            //Uri destinationPath = new Uri("C:\\Users\\dse41_mi11\\Documents\\OU\\D-70\\motion_extraction-main\\pillow\\red_triangle.jpg");
            //Mat destination = Cv2.ImRead(destinationPath.AbsolutePath, ImreadModes.Color);
            //MotionExtraction.XOR(source, destination);
        }

        /// <summary>
        /// Initiates the playback animation for the images in the Images collection.
        /// The playback can be forward or reverse based on the IsReversePlayback property.
        /// </summary>
        private async void OnPlay()
        {
            _isAnimating = true;

            while (_isAnimating)
            {
                await Task.Delay(TimeDelay); // Default 250

                if (!_isAnimating)
                    break;

                if (IsReversePlayback)
                {
                    CurrentIndex = (CurrentIndex - 1 + Images.Count) % Images.Count; // Handle reverse playback
                }
                else
                {
                    CurrentIndex = (CurrentIndex + 1) % Images.Count; // Handle forward playback
                }

                CurrentImage = Images[CurrentIndex];
            }
        }

        /// <summary>
        /// Stops the playback animation by setting the animating flag to false.
        /// </summary>
        private void OnStop()
        {
            _isAnimating = false;
        }

        /// <summary>
        /// Moves to the previous image in the Images collection.
        /// Stops any ongoing playback animation.
        /// </summary>
        private void OnPrevious()
        {
            if (Images.Count == 0)
                return;

            _isAnimating = false;
            CurrentIndex = (CurrentIndex - 1 + Images.Count) % Images.Count;
            CurrentImage = Images[CurrentIndex];
        }

        /// <summary>
        /// Moves to the next image in the Images collection.
        /// Stops any ongoing playback animation.
        /// </summary>
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
