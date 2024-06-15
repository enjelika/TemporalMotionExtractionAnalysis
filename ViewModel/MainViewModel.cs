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
        /// Updates the selected images by adjusting brightness, contrast, and saturation using OpenCV.
        /// The modified images are saved to a temporary folder, and the ModifiedImagePath property
        /// of each selected frame is updated accordingly.
        /// </summary>
        /// <param name="brightness">The brightness adjustment value (range: 0-100, where 50 means no change).</param>
        /// <param name="contrast">The contrast adjustment value (range: 0-100, where 50 means no change).</param>
        /// <param name="saturation">The saturation adjustment value (range: 0-100, where 50 means no change).</param>
        /// <remarks>
        /// This function processes the images using OpenCV for brightness, contrast, and saturation adjustments.
        /// It converts the brightness and contrast values to OpenCV ranges, applies the adjustments, and saves the
        /// modified images to a temporary folder. The ModifiedImagePath property of each ImageModel in SelectedFrames
        /// is updated to point to the saved file.
        /// </remarks>
        private void UpdateSelectedImage(int brightness, int contrast, int saturation)
        {
            if (SelectedFrames != null && SelectedFrames.Any() && !string.IsNullOrEmpty(SelectedFrames[0].ImagePath))
            {
                int counter = 1;

                foreach (var frame in SelectedFrames)
                {
                    Mat image = Cv2.ImRead(frame.ImagePath);

                    // Convert brightness, contrast, and saturation to OpenCV ranges
                    double alpha = contrast / 100.0 + 1.0; // Contrast factor (1.0 means no change)
                    double beta = brightness - 50;         // Brightness offset (-255 to 255)
                    double saturationFactor = saturation / 50.0; // Saturation factor (0 means grayscale, 1 means original, >1 means more saturated)

                    // Apply brightness and contrast
                    Mat newImage = new Mat();
                    image.ConvertTo(newImage, MatType.CV_8UC3, alpha, beta);

                    // Convert to HSV to adjust saturation
                    Cv2.CvtColor(newImage, newImage, ColorConversionCodes.BGR2HSV);
                    var channels = new Mat[3];
                    Cv2.Split(newImage, out channels);
                    channels[1] = channels[1] * saturationFactor;
                    Cv2.Merge(channels, newImage);
                    Cv2.CvtColor(newImage, newImage, ColorConversionCodes.HSV2BGR);

                    // Save modified image to a temporary folder
                    string tempFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
                    Directory.CreateDirectory(tempFolderPath);
                    string tempFileName = $"{System.IO.Path.GetFileNameWithoutExtension(frame.ImagePath)}_mod{counter}.jpg";
                    string tempFilePath = System.IO.Path.Combine(tempFolderPath, tempFileName);
                    Cv2.ImWrite(tempFilePath, newImage);

                    // Update the ModifiedImagePath
                    frame.ModifiedPicturePath = tempFilePath;
                    OnPropertyChanged(nameof(frame.ModifiedPicturePath));

                    counter++;
                }
            }
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

                        // Check to see if there is an existing ModifiedPicturePath
                        //   Set to the original image path if not modified yet
                        if (imageModel.ModifiedPicturePath == null)
                        {
                            imageModel.ModifiedPicturePath = imageModel.ImagePath;
                            OnPropertyChanged(imageModel.ModifiedPicturePath);
                        }
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
