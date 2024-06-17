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
using System.Windows;

namespace TemporalMotionExtractionAnalysis.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Variables
        private ObservableCollection<ImageModel> _images;
        private ObservableCollection<ImageModel> _timelineCells;
        private ObservableCollection<ImageModel> _zoomedTimelineCells;
        private ObservableCollection<ImageModel> _selectedFrames;
        private ObservableCollection<string> _fpsValue;

        private ImageModel _previousImage;
        private ImageModel _currentImage;
        private ImageModel _nextImage;
        private ImageModel _offsetFrame;
        private ImageModel _compositiedImage;

        private int _imageCount;
        private int _currentIndex;
        private int _offsetValue;
        private int _timeDelay;
        private int _selectedFps;

        private const int TotalCells = 57;
        private const int CenterIndex = 26;

        private bool _isAnimating;
        private bool _isReversePlayback;
        private bool _isImagesLoaded;

        private string _folderName;
        private string _compositeImageLocation;

        private CompositionMode _selectedCompositionMode;
        #endregion

        #region Enums
        public enum CompositionMode
        {
            SourceOver,     // 1
            DestinationOver,// 2
            SourceIn,       // 3
            DestinationIn,  // 4
            SourceOut,      // 5
            DestinationOut, // 6
            SourceAtop,     // 7
            DestinationAtop,// 8
            Clear,          // 9
            XOR             // 10
        }
        #endregion

        #region Public Variables
        public ObservableCollection<ImageModel> Images
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChanged(nameof(Images));
            }
        }

        public ImageModel CompositedImage
        {
            get => _compositiedImage;
            set
            {
                _compositiedImage = value;
                OnPropertyChanged(nameof(CompositedImage));
            }
        }

        public ObservableCollection<ImageModel> TimelineCells
        {
            get => _timelineCells;
            set 
            { 
                _timelineCells = value;
                OnPropertyChanged(nameof(TimelineCells)); 
            }
        }

        public ObservableCollection <ImageModel> ZoommedTimelineCells
        {
            get => _zoomedTimelineCells;
            set
            {
                _zoomedTimelineCells = value;
                OnPropertyChanged(nameof(ZoommedTimelineCells));
            }
        }

        public CompositionMode SelectedCompositionMode
        {
            get => _selectedCompositionMode;
            set
            {
                _selectedCompositionMode = value;
                Console.WriteLine("Selected Composition Mode: " + _selectedCompositionMode.ToString());
                OnPropertyChanged(nameof(CompositionMode));

                // Trigger composition whenever the mode changes
                ComposeImages();
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

        public ImageModel OffsetFrame
        {
            get => _offsetFrame;
            set
            {
                _offsetFrame = value;
                Console.WriteLine(OffsetFrame.ImagePath); // Debugging
                OnPropertyChanged(nameof(OffsetValue));
            }
        }

        public ObservableCollection<string> FpsValue
        {
            get => _fpsValue;
            set
            {
                if (_fpsValue != value)
                {
                    _fpsValue = value;
                    OnPropertyChanged(nameof(FpsValue)); // Notify property changed
                                                         // Update the time delay based on the new FPS value
                }
            }
        }

        public int SelectedFps
        {
            get => _selectedFps;
            set
            {
                _selectedFps = value;
                Console.WriteLine(SelectedFps.ToString()); // Debugging
                OnPropertyChanged(nameof(SelectedFps));
                TimeDelay = ConvertFPSToTimeDelay(_selectedFps);
                OnPropertyChanged(nameof(TimeDelay));
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

        public ImageModel PreviousImage
        {
            get => _previousImage;
            set
            {
                _previousImage = value;
                OnPropertyChanged(nameof(PreviousImage));
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

        public ImageModel NextImage
        {
            get => _nextImage;
            set
            {
                _nextImage = value;
                OnPropertyChanged(nameof(NextImage));
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

        public bool IsImagesLoaded
        {
            get => _isImagesLoaded;
            set
            {
                _isImagesLoaded = value;
                OnPropertyChanged(nameof(IsImagesLoaded));
            }
        }
        #endregion

        #region ICommands
        public ICommand LoadImagesCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand StartMotionExtractionCommand { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// Sets up initial state, commands, and collections.
        /// </summary>
        public MainViewModel()
        {
            // Initialize the variables for the View Textboxes & Combobox
            OffsetValue = 0; // Default value
            TimeDelay = 250; // Default value
            FpsValue = new ObservableCollection<string>() { "4", "10", "20", "30", "40", "60" }; 
            SelectedFps = 4; // Default value is 4
            FolderName = "No Selected Folder";
            SelectedFrames = new ObservableCollection<ImageModel>();

            // Initialize the ObservableCollection<ImageModel> for the Images
            Images = new ObservableCollection<ImageModel>();
            TimelineCells = new ObservableCollection<ImageModel>();
            ZoommedTimelineCells = new ObservableCollection<ImageModel>();

            // Initialize timeline cells with default values
            for (int i = 0; i < TotalCells; i++)
            {
                TimelineCells.Add(new ImageModel
                {
                    FrameNumber = -1, // Initialize with -1 indicating empty
                    IsCurrent = false
                });
            }

            // Initialize zoomed timeline cells with default values
            for (int i = 0; i < 5; i++)
            {
                ZoommedTimelineCells.Add(new ImageModel
                {
                    FrameNumber = -1, // Initialize with -1 indicating empty
                    IsCurrent = false
                });
            }

            // Set initial current frame - Timeline Overview
            SetCurrentFrame(0);

            OffsetFrame = new ImageModel();
            OffsetFrame.IsOffsetSelection = false; 

            // Default false since no images have been loaded yet
            IsImagesLoaded = false;

            // Initialize the Commands
            LoadImagesCommand = new RelayCommand(LoadImages);
            PlayCommand = new RelayCommand(OnPlay);
            PauseCommand = new RelayCommand(OnStop);
            StopCommand = new RelayCommand(OnStop);
            PreviousCommand = new RelayCommand(OnPrevious);
            NextCommand = new RelayCommand(OnNext);
            StartMotionExtractionCommand = new RelayCommand(StartMotionExtraction);
            SelectedCompositionMode = CompositionMode.SourceOver;

            // Initialize commands or event handlers for composition mode changes
            // For simplicity, assume there's a PropertyChanged event handler wired up to trigger composition
            PropertyChanged += OnCompositionModeChanged;
        }

        /// <summary>
        /// Sets the current frame in the timeline and updates the timeline cells accordingly.
        /// </summary>
        /// <param name="currentFrameIndex">The index of the current frame.</param>
        public void SetCurrentFrame(int currentFrameIndex)
        {
            // Check if Images collection is valid and non-empty
            if (Images == null || !Images.Any())
                return;

            int totalFrames = Images.Count;

            // Calculate the start index based on the desired center index
            int startIndex = currentFrameIndex - CenterIndex;
            if (startIndex < 0)
            {
                startIndex += totalFrames; // Wrap around for negative indices
            }

            // Update TimelineCells with frame numbers and current frame indicator
            for (int i = 0; i < TotalCells; i++)
            {
                int frameIndex = (startIndex + i + totalFrames) % totalFrames;
                TimelineCells[i].FrameNumber = frameIndex;
                TimelineCells[i].ImagePath = Images[frameIndex].ImagePath;
                TimelineCells[i].IsCurrent = (i == CenterIndex); // Set IsCurrent for center cell

                if (TimelineCells[i].IsCurrent) 
                {
                    // Set the image for the previous frame and next frame in TimelineCells
                    int prevFrameIndex = (frameIndex - 1 + totalFrames) % totalFrames;
                    int nextFrameIndex = (frameIndex + 1) % totalFrames;

                    PreviousImage = Images[prevFrameIndex];
                    NextImage = Images[nextFrameIndex];
                }
            }

            // Notify property changed for TimelineCells to update the UI
            OnPropertyChanged(nameof(TimelineCells));
        }

        /// <summary>
        /// Sets the current frame in the zoomed timeline and updates the zoomed timeline cells accordingly.
        /// </summary>
        /// <param name="currentFrameIndex">The index of the current frame.</param>
        public void SetZoomedCurrentFrame(int currentFrameIndex)
        {
            // Check if Images collection is valid and non-empty
            if (Images == null || !Images.Any())
                return;

            int totalFrames = Images.Count;
            int centerCellIndex = 2; // Center cell is always at index 2

            // Calculate the start index based on the desired center index
            int startIndex = currentFrameIndex - centerCellIndex;
            if (startIndex < 0)
            {
                startIndex += totalFrames; // Wrap around for negative indices
            }

            // Update ZoomedTimelineCells with frame numbers and current frame indicator
            for (int i = 0; i < ZoommedTimelineCells.Count; i++)
            {
                int frameIndex = (startIndex + i + totalFrames) % totalFrames;
                ZoommedTimelineCells[i].FrameNumber = frameIndex;
                ZoommedTimelineCells[i].ImagePath = Images[frameIndex].ImagePath;
                ZoommedTimelineCells[i].IsCurrent = (i == centerCellIndex); // Center cell is always at index 2

                if (ZoommedTimelineCells[i].IsCurrent)
                {
                    // Set the image for the previous frame and next frame in TimelineCells
                    int prevFrameIndex = (frameIndex - 1 + totalFrames) % totalFrames;
                    int nextFrameIndex = (frameIndex + 1) % totalFrames;

                    PreviousImage = Images[prevFrameIndex];
                    NextImage = Images[nextFrameIndex];
                }
            }

            // Notify property changed for ZoomedTimelineCells to update the UI
            OnPropertyChanged(nameof(ZoommedTimelineCells));
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
                        Images.Add(new ImageModel 
                        { 
                            ImagePath = file, 
                            FrameNumber = frameNumber,
                            IsCurrent = frameNumber == 0 // Assuming 0 is the initial current frame

                        });
                        

                        frameNumber++;
                    }

                    // Set initial current frame
                    if (Images.Any())
                    {
                        SetCurrentFrame(0);
                        SetZoomedCurrentFrame(0);
                        CurrentImage = Images.First();
                        CurrentIndex = 0;
                    }

                    FolderName = System.IO.Path.GetFileName(selectedPath);

                    IsImagesLoaded = true;
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
                SetCurrentFrame(CurrentIndex); // Update the timeline cells
                SetZoomedCurrentFrame(CurrentIndex); // Update the zoomed timeline cells

                //if (SelectedFrames.Count == 2)
                //{
                //    ComposeImages();
                //}
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
            SetCurrentFrame(CurrentIndex); // Update the timeline cells
            SetZoomedCurrentFrame(CurrentIndex); // Update the zoomed timeline cells

            //if(SelectedFrames.Count == 2)
            //{
            //    ComposeImages();
            //}
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
            SetCurrentFrame(CurrentIndex); // Update the timeline cells
            SetZoomedCurrentFrame(CurrentIndex); // Update the zoomed timeline cells

            //if (SelectedFrames.Count == 2)
            //{
            //    ComposeImages();
            //}
        }

        // Composition logic based on the selected mode
        private void ComposeImages()
        {
            string tempFolderPath = System.IO.Path.GetTempPath();

            // Call the SourceOver method from the model
            if (SelectedFrames != null && SelectedFrames.Count >= 2)
            {
                // Get the image paths for source and destination images
                string sourceImagePath = SelectedFrames[0].ImagePath;
                string destinationImagePath = SelectedFrames[1].ImagePath;

                // Load the images from their paths
                Mat sourceImage = Cv2.ImRead(sourceImagePath);
                Mat destinationImage = Cv2.ImRead(destinationImagePath);

                // Motion Extraction of the sourceImage and destinationImage
                MotionExtraction motionExtraction = new MotionExtraction();

                // Step 1: Invert colors
                Mat invertSourceImage = motionExtraction.InvertColors(sourceImage);
                Mat invertDestinationImage = motionExtraction.InvertColors(destinationImage);

                // Step 2: Reduce Alpha/Opacity
                Mat reducedAlphaSourceImage = motionExtraction.ReduceAlpha(invertSourceImage);
                Mat reducedAlphaDestinationImage = motionExtraction.ReduceAlpha(invertDestinationImage);

                // Step 3: Add Blur
                OpenCvSharp.Size kernelSize = new OpenCvSharp.Size(7,7); // Medium size: 7x7 kernel
                Mat blurSourceImage = motionExtraction.BlurImage(reducedAlphaSourceImage, kernelSize);
                Mat blurDestinationImage = motionExtraction.BlurImage(reducedAlphaDestinationImage, kernelSize);

                // Call the SourceOver method from the model
                switch (SelectedCompositionMode)
                {
                    case CompositionMode.SourceOver:
                        Mat composedImage = motionExtraction.SourceOver(blurSourceImage, blurDestinationImage);

                        // Save the composed image as a PNG in a temporary folder
                        string composedImagePath = System.IO.Path.Combine(tempFolderPath, "ComposedImage.png");
                        Cv2.ImWrite(composedImagePath, composedImage);

                        // Create a new ImageModel object with the composed image path
                        ImageModel composedImageModel = new ImageModel { ImagePath = composedImagePath };

                        // Update ViewModel properties or raise events as needed
                        CompositedImage = composedImageModel;
                        OnPropertyChanged(nameof(CompositedImage));
                        break;
                    case CompositionMode.DestinationOver:
                        Mat composedImage2 = motionExtraction.DestinationOver(blurSourceImage, blurDestinationImage);

                        // Save the composed image as a PNG in a temporary folder
                        string composedImagePath2 = System.IO.Path.Combine(tempFolderPath, "ComposedImage.png");
                        Cv2.ImWrite(composedImagePath2, composedImage2);

                        // Create a new ImageModel object with the composed image path
                        ImageModel composedImageModel2 = new ImageModel { ImagePath = composedImagePath2 };

                        // Update ViewModel properties or raise events as needed
                        CompositedImage = composedImageModel2;
                        OnPropertyChanged(nameof(CompositedImage));
                        break;
                    case CompositionMode.SourceIn:
                        Mat composedImage3 = motionExtraction.SourceIn(blurSourceImage, blurDestinationImage);

                        // Save the composed image as a PNG in a temporary folder
                        string composedImagePath3 = System.IO.Path.Combine(tempFolderPath, "ComposedImage.png");
                        Cv2.ImWrite(composedImagePath3, composedImage3);

                        // Create a new ImageModel object with the composed image path
                        ImageModel composedImageModel3 = new ImageModel { ImagePath = composedImagePath3 };

                        // Update ViewModel properties or raise events as needed
                        CompositedImage = composedImageModel3;
                        OnPropertyChanged(nameof(CompositedImage));
                        break;
                    case CompositionMode.DestinationIn:
                        Mat composedImage4 = motionExtraction.DestinationIn(blurSourceImage, blurDestinationImage);

                        // Save the composed image as a PNG in a temporary folder
                        string composedImagePath4 = System.IO.Path.Combine(tempFolderPath, "ComposedImage.png");
                        Cv2.ImWrite(composedImagePath4, composedImage4);

                        // Create a new ImageModel object with the composed image path
                        ImageModel composedImageModel4 = new ImageModel { ImagePath = composedImagePath4 };

                        // Update ViewModel properties or raise events as needed
                        CompositedImage = composedImageModel4;
                        OnPropertyChanged(nameof(CompositedImage));
                        break;
                    case CompositionMode.SourceOut:
                        Mat composedImage5 = motionExtraction.SourceOut(blurSourceImage, blurDestinationImage);

                        // Save the composed image as a PNG in a temporary folder
                        string composedImagePath5 = System.IO.Path.Combine(tempFolderPath, "ComposedImage.png");
                        Cv2.ImWrite(composedImagePath5, composedImage5);

                        // Create a new ImageModel object with the composed image path
                        ImageModel composedImageModel5 = new ImageModel { ImagePath = composedImagePath5 };

                        // Update ViewModel properties or raise events as needed
                        CompositedImage = composedImageModel5;
                        OnPropertyChanged(nameof(CompositedImage));
                        break;
                    // Add cases for other composition modes as needed
                    default:
                        break;
                }
            }
        }

        // Event handler for composition mode changes
        private void OnCompositionModeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedCompositionMode))
            {
                ComposeImages();
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
            // Reset all selections
            foreach (var image in TimelineCells)
            {
                if (selectedItems.Contains(image))
                {
                    image.IsOffsetSelection = true;
                }
                else
                {
                    image.IsOffsetSelection = false;
                }
            }

            if (selectedItems.Count <= 2)
            {
                SelectedFrames.Clear();
                foreach (var item in selectedItems)
                {
                    if (item is ImageModel imageModel)
                    {
                        // Current Frame
                        SelectedFrames.Add(Images[CurrentIndex]);
                        SelectedFrames[0].IsOffsetSelection = true;
                       
                        // User Selected Offset Frame
                        SelectedFrames.Add(imageModel);
                        SelectedFrames[1].IsOffsetSelection = true;
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
