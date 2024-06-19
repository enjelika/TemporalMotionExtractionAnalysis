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
using System.Windows.Media;
using System.Drawing;
using System;
using System.Diagnostics.Eventing.Reader;

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
        private ObservableCollection<string> _blurValue;
        private ObservableCollection<string> _sourceColors;
        private ObservableCollection<string> _destinationColors;
        private ObservableCollection<string> _compositionModes;
        private ObservableCollection<string> _backgroundMarksTextures;

        private ImageModel _previousImage;
        private ImageModel _currentImage;
        private ImageModel _nextImage;
        private ImageModel _offsetFrame;
        private ImageModel _compositiedImage;

        private System.Windows.Media.Color _selectedSourceColor;
        private System.Windows.Media.Color _selectedDestinationColor;
        private SolidColorBrush _selectedSourceBrush;
        private SolidColorBrush _selectedDestinationBrush;

        private int _imageCount;
        private int _currentIndex;
        private int _offsetValue;
        private int _timeDelay;
        private int _selectedFps;

        private double _calculatedEmeasure;
        private double _calculatedMAE;
        private double _calculatedSSIM;

        private const int TotalCells = 120; //57
        private const int CenterIndex = 2; //26

        private bool _isAnimating;
        private bool _isReversePlayback;
        private bool _isImagesLoaded;

        private bool _isForePixelModeSelected;
        private bool _isForeMarksModeSelected;
        private bool _isBackPixelModeSelected;
        private bool _isBackMarksModeSelected;

        private bool _isCurrentXORSelected;
        private bool _isOffsetXORSelected;

        private string _folderName;
        private string _compositeImageLocation;
        private string _transformedCurrentImageLocation;
        private string _transformedOffsetImageLocation;

        private ObservableCollection<string> composedImagePaths = new ObservableCollection<string>();
        private ObservableCollection<string> transformedCurrentImagePaths = new ObservableCollection<string>();
        private ObservableCollection<string> transformedOffsetImagePaths = new ObservableCollection<string>();

        private string _selectedForegroundCompositionMode;
        private string _selectedBackgroundCompositionMode;
        private string _selectedBackgroundMarksTextures;

        private GlyphRendering glyphRendering;

        private string _positiveGlyph = "▲";
        private string _negativeGlyph = "▼";
        private string _noDifferenceGlyph = "■";
        private int _areaSize;
        private int _currentBlurSize;
        private int _offsetBlurSize;

        private OpenCvSharp.Size _currentKernelSize;
        private OpenCvSharp.Size _offsetKernelSize;
        #endregion

        #region Public Variables

        #region Color / Brushes
        public System.Windows.Media.Color SelectedSourceColor
        {
            get => _selectedSourceColor;
            set
            {
                _selectedSourceColor = value;
                OnPropertyChanged(nameof(SelectedSourceColor));
                OnPropertyChanged(nameof(SelectedSourceBrush));
            }
        }

        public System.Windows.Media.Color SelectedDestinationColor
        {
            get => _selectedDestinationColor;
            set
            {
                _selectedDestinationColor = value;
                OnPropertyChanged(nameof(SelectedDestinationColor));
                OnPropertyChanged(nameof(SelectedDestinationBrush));
            }
        }

        public SolidColorBrush SelectedSourceBrush
        {
            get { return new SolidColorBrush(SelectedSourceColor); }
        }

        public SolidColorBrush SelectedDestinationBrush
        {
            get { return new SolidColorBrush(SelectedDestinationColor); }
        }
        #endregion

        #region ObservableCollections
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

        public ObservableCollection<string> ComposedImagePaths
        {
            get { return composedImagePaths; }
            set { composedImagePaths = value; OnPropertyChanged(nameof(ComposedImagePaths)); }
        }

        public ObservableCollection<string> TransformedCurrentImagePaths
        {
            get { return transformedCurrentImagePaths; }
            set { transformedCurrentImagePaths = value; OnPropertyChanged(nameof(TransformedCurrentImagePaths)); }
        }

        public ObservableCollection<string> TransformedOffsetImagePaths
        {
            get { return transformedOffsetImagePaths; }
            set { transformedOffsetImagePaths = value; OnPropertyChanged(nameof(TransformedOffsetImagePaths)); }
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

        public ObservableCollection<string> BlurValue
        {
            get => _blurValue;
            set
            {
                if (_blurValue != value)
                {
                    _blurValue = value;
                    OnPropertyChanged(nameof(BlurValue)); // Notify property changed
                                                          // Update the time delay based on the new Blur value
                }
            }
        }

        public ObservableCollection<string> SourceColors
        {
            get => _sourceColors;
            set
            {
                if (_sourceColors != value)
                {
                    _sourceColors = value;
                    OnPropertyChanged(nameof(SourceColors)); // Notify property changed
                }
            }
        }

        public ObservableCollection<string> DestinationColors
        {
            get => _destinationColors;
            set
            {
                if (_destinationColors != value)
                {
                    _destinationColors = value;
                    OnPropertyChanged(nameof(DestinationColors)); // Notify property changed
                }
            }
        }

        public ObservableCollection<string> CompositionModes
        {
            get => _compositionModes;
            set
            {
                if (_compositionModes != value)
                {
                    _compositionModes = value;
                    OnPropertyChanged(nameof(CompositionModes)); // Notify property changed
                }
            }
        }

        public ObservableCollection<string> BackgroundMarksTextures
        {
            get => _backgroundMarksTextures;
            set
            {
                if (_backgroundMarksTextures != value)
                {
                    _backgroundMarksTextures = value;
                    OnPropertyChanged(nameof(BackgroundMarksTextures)); // Notify property changed
                }
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
        #endregion

        public string SelectedForegroundCompositionMode
        {
            get => _selectedForegroundCompositionMode;
            set
            {
                _selectedForegroundCompositionMode = value;
                Console.WriteLine("Selected Foreground Composition Mode: " + _selectedForegroundCompositionMode.ToString());
                OnPropertyChanged(nameof(SelectedForegroundCompositionMode));

                // Trigger composition whenever the mode changes
                ComposeImages();
            }
        }

        public string SelectedBackgroundCompositionMode
        {
            get => _selectedBackgroundCompositionMode;
            set
            {
                _selectedBackgroundCompositionMode = value;
                Console.WriteLine("Selected Background Composition Mode: " + _selectedBackgroundCompositionMode.ToString());
                OnPropertyChanged(nameof(SelectedBackgroundCompositionMode));

                // Trigger composition whenever the mode changes
                ComposeImages();
            }
        }

        public string SelectedBackgroundMarksTextures
        {
            get => _selectedBackgroundMarksTextures;
            set
            {
                _selectedBackgroundMarksTextures = value;
                Console.WriteLine("Selected Background Marks Textures: " + _selectedBackgroundMarksTextures.ToString());
                OnPropertyChanged(nameof(SelectedBackgroundMarksTextures));

                // Trigger composition whenever the mode changes
                ComposeImages();
            }
        }

        public double CalculatedEmeasure
        {
            get => _calculatedEmeasure;
            set 
            { 
                _calculatedEmeasure = value;
                OnPropertyChanged(nameof(CalculatedEmeasure));
            }
        }

        public double CalculatedMAE
        {
            get => _calculatedMAE;
            set
            {
                _calculatedMAE = value;
                OnPropertyChanged(nameof(CalculatedMAE));
            }
        }

        public double CalculatedSSIM
        {
            get => _calculatedSSIM;
            set
            {
                _calculatedSSIM = value;
                OnPropertyChanged(nameof(CalculatedSSIM));
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

        

        public int SelectedFps
        {
            get => _selectedFps;
            set
            {
                _selectedFps = value;
                Console.WriteLine(SelectedFps.ToString()); // Debugging
                OnPropertyChanged(nameof(SelectedFps));
                TimeDelay = ConvertFPSToTimeDelay(_selectedFps);
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

        #region Boolean Flags
        public bool IsReversePlayback
        {
            get => _isReversePlayback;
            set
            {
                _isReversePlayback = value;
                OnPropertyChanged(nameof(IsReversePlayback));
            }
        }

        public bool IsForePixelModeSelected
        {
            get => _isForePixelModeSelected; 
            set 
            { 
                _isForePixelModeSelected = value; OnPropertyChanged(nameof(IsForePixelModeSelected)); 
            }
        }

        public bool IsForeMarksModeSelected
        {
            get => _isForeMarksModeSelected; 
            set 
            {
                _isForeMarksModeSelected = value; OnPropertyChanged(nameof(IsForeMarksModeSelected)); 
            }
        }

        public bool IsBackPixelModeSelected
        {
            get => _isBackPixelModeSelected;
            set
            {
                _isBackPixelModeSelected = value; OnPropertyChanged(nameof(IsBackPixelModeSelected));
            }
        }

        public bool IsBackMarksModeSelected
        {
            get => _isBackMarksModeSelected;
            set
            {
                _isBackMarksModeSelected = value; OnPropertyChanged(nameof(IsBackMarksModeSelected));
            }
        }

        public bool IsCurrentXORSelected
        {
            get => _isCurrentXORSelected;
            set
            {
                _isCurrentXORSelected = value;
                OnPropertyChanged(nameof(IsCurrentXORSelected));
            }
        }

        public bool IsOffsetXORSelected
        {
            get => _isOffsetXORSelected;
            set
            {
                _isOffsetXORSelected = value;
                OnPropertyChanged(nameof(IsOffsetXORSelected));
            }
        }
        #endregion

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

        public bool IsImagesLoaded
        {
            get => _isImagesLoaded;
            set
            {
                _isImagesLoaded = value;
                OnPropertyChanged(nameof(IsImagesLoaded));
            }
        }

        public string PositiveGlyph
        {
            get => _positiveGlyph;
            set
            {
                _positiveGlyph = value;
                OnPropertyChanged(nameof(PositiveGlyph));
            }
        }

        public string NegativeGlyph
        {
            get => _negativeGlyph; 
            set
            {
                _negativeGlyph = value;
                OnPropertyChanged(nameof(NegativeGlyph));
            }
        }

        public string NoDifferenceGlyph
        {
            get => _noDifferenceGlyph; 
            set
            {
                _noDifferenceGlyph = value;
                OnPropertyChanged(nameof(NoDifferenceGlyph));
            }
        }

        public int AreaSize
        {
            get => _areaSize;
            set
            {
                _areaSize = value;
                OnPropertyChanged(nameof(AreaSize));
            }
        }

        public int SelectedCurrentBlurSize
        {
            get => _currentBlurSize;
            set
            {
                _currentBlurSize = value;
                OnPropertyChanged(nameof(SelectedCurrentBlurSize)); // Notify property changed
                UpdateCurrentKernelSize();
            }
        }

        public int SelectedOffsetBlurSize
        {
            get => _offsetBlurSize;
            set
            {
                _offsetBlurSize = value;
                OnPropertyChanged(nameof(SelectedOffsetBlurSize)); // Notify property changed
                UpdateOffsetKernelSize();
            }
        }

        public OpenCvSharp.Size CurrentKernelSize
        {
            get => _currentKernelSize;
            private set
            {
                _currentKernelSize = value;
                OnPropertyChanged(nameof(CurrentKernelSize));
            }
        }

        public OpenCvSharp.Size OffsetKernelSize
        {
            get => _offsetKernelSize;
            private set
            {
                _offsetKernelSize = value;
                OnPropertyChanged(nameof(OffsetKernelSize));
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
            // Initialize the Color values
            SelectedSourceColor = System.Windows.Media.Color.FromRgb(255,0,0); //Red
            SelectedDestinationColor = System.Windows.Media.Color.FromRgb(0,0,255); //Blue

            // Initialize the variables for the View Textboxes & Combobox
            OffsetValue = 0; // Default value
            TimeDelay = 250; // Default value
            FpsValue = new ObservableCollection<string>() { "4", "10", "20", "30", "40", "60" };
            BlurValue = new ObservableCollection<string>() { "5", "7", "9" };
            SelectedFps = 4; // Default value is 4
            FolderName = "No Selected Folder";
            SelectedFrames = new ObservableCollection<ImageModel>();
            SelectedCurrentBlurSize = 7; // Default value is 7
            SelectedOffsetBlurSize = 7;
            AreaSize = 40; // Default value
            SourceColors = new ObservableCollection<string>() { "Red", "Yellow", "Blue" };
            DestinationColors = new ObservableCollection<string>() { "Orange", "Green", "Purple" };
            CompositionModes =  new ObservableCollection<string>() { "SourceOver", "DestinationOver", "SourceIn", 
                "DestinationIn", "SourceOut", "DestinationOut", "SourceAtop", "DestinationAtop", "Clear", "XOR" };
            BackgroundMarksTextures = new ObservableCollection<string>() { "Texture1", "Texture2", "Texture3" };

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
            SelectedForegroundCompositionMode = "SourceOver";

            // Initialize the GlyphRendering class with default glyphs
            glyphRendering = new GlyphRendering("▲", "▼", "■", AreaSize);

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
        /// Resets the IsOffsetSelection property of all TimelineCells to false.
        /// </summary>
        public void ResetOffsetSelection()
        {
            foreach (var cell in TimelineCells)
            {
                cell.IsOffsetSelection = false;
            }
        }

        /// <summary>
        /// Converts frames per second (FPS) to a time delay in milliseconds.
        /// </summary>
        /// <param name="fps">The frames per second value to convert.</param>
        /// <returns>The corresponding time delay in milliseconds.</returns>
        public int ConvertFPSToTimeDelay(int fps)
        {
            if (fps > 4) 
            { 
                return (int)(1000.0 / fps); // Convert FPS to time delay in milliseconds
            }
            else
            {
                return 250;
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

                    if(SelectedFrames.Count >= 2) 
                    {
                        ResetOffsetSelection();
                        CompositedImage.ImagePath = "";
                        SelectedFrames.Clear();
                    }

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

        #region Animation Controls
        /// <summary>
        /// Initiates the playback animation for the images in the Images collection.
        /// The playback can be forward or reverse based on the IsReversePlayback property.
        /// </summary>
        private async void OnPlay()
        {
            _isAnimating = true;

            while (_isAnimating)
            {
                await Task.Delay(TimeDelay); // Default 250 aka FPS 4

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

                if (SelectedFrames.Count == 2)
                {
                    ComposeImages();
                }
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

            if(SelectedFrames.Count == 2)
            {
                ComposeImages();
            }
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

            if (SelectedFrames.Count == 2)
            {
                ComposeImages();
            }
        }
        #endregion

        // Method to update glyphs from the View
        public void UpdateGlyphs(string positive, string negative, string noDifference)
        {
            glyphRendering.PositiveGlyph = positive;
            glyphRendering.NegativeGlyph = negative;
            glyphRendering.NoDifferenceGlyph = noDifference;
        }

        private void UpdateCurrentKernelSize()
        { 
            // Update the OpenCv.Kernel.Size based on the selected blur size
            // Small size:  5x5 kernel
            // Medium size: 7x7 kernel
            // Medium size: 9x9 kernel
            CurrentKernelSize = new OpenCvSharp.Size(SelectedCurrentBlurSize, SelectedCurrentBlurSize);
        }

        private void UpdateOffsetKernelSize()
        {
            // Update the OpenCv.Kernel.Size based on the selected blur size
            // Small size:  5x5 kernel
            // Medium size: 7x7 kernel
            // Medium size: 9x9 kernel
            OffsetKernelSize = new OpenCvSharp.Size(SelectedOffsetBlurSize, SelectedOffsetBlurSize);
        }

        private void ProcessCurrentTransforms()
        {
            // Get the image paths for source
            string currentImagePath = CurrentImage.ImagePath;

            // Load the images from their paths
            Mat currentImage = Cv2.ImRead(currentImagePath);

            // Motion Extraction of the current image
            MotionExtraction motionExtraction = new MotionExtraction();

            Mat invertCurrentImage = new Mat(currentImage);

            // Step 1: Invert colors (if selected)
            if (IsCurrentXORSelected)
            {
                invertCurrentImage = motionExtraction.InvertColors(currentImage);
            }
            else
            {
                invertCurrentImage = currentImage;
            }

            // Step 2: Reduce Alpha/Opacity
            Mat reducedAlphaCurrentImage = motionExtraction.ReduceAlpha(invertCurrentImage);

            // Step 3: Add Blur
            Mat blurCurrentImage = motionExtraction.BlurImage(reducedAlphaCurrentImage, CurrentKernelSize);

        }

        // Composition logic based on the selected mode
        private void ComposeImages()
        {
            // Call the Composition Mode method from the model
            if (SelectedFrames != null && SelectedFrames.Count >= 2)
            {
            //    // Get the image paths for source and destination images
            //    string sourceImagePath = CurrentImage.ImagePath;
            //    string destinationImagePath = SelectedFrames[1].ImagePath;

            //    // Load the images from their paths
            //    Mat sourceImage = Cv2.ImRead(sourceImagePath);
            //    Mat destinationImage = Cv2.ImRead(destinationImagePath);

            //    // Motion Extraction of the sourceImage and destinationImage
            //    MotionExtraction motionExtraction = new MotionExtraction();
            //    CompositionModeRendering compositeModeRendering = new CompositionModeRendering();

            //    // Step 1: Invert colors
            //    Mat invertSourceImage = motionExtraction.InvertColors(sourceImage);
            //    Mat invertDestinationImage = motionExtraction.InvertColors(destinationImage);

            //    // Step 2: Reduce Alpha/Opacity
            //    Mat reducedAlphaSourceImage = motionExtraction.ReduceAlpha(invertSourceImage);
            //    Mat reducedAlphaDestinationImage = motionExtraction.ReduceAlpha(invertDestinationImage);

            //    // Step 3: Add Blur
                
            //    Mat blurDestinationImage = motionExtraction.BlurImage(reducedAlphaDestinationImage, kernelSize);

            //    CalculatedEmeasure = Math.Round(motionExtraction.CalculateEmeasurePixelwise(blurSourceImage, blurDestinationImage), 4);
            //    CalculatedMAE = Math.Round(motionExtraction.CalculateMAE(blurSourceImage, blurDestinationImage), 4);
            //    CalculatedSSIM = Math.Round(motionExtraction.CalculateSSIM(blurSourceImage, blurDestinationImage), 4);

            //    // Step 4: Tint the Source red, and the Destination blue (temporary until color selector controls are added)
            //    Mat tintedSourceImage = ApplyColorTint(blurSourceImage, SelectedSourceBrush);
            //    Mat tintedDestinationImage = ApplyColorTint(blurDestinationImage, SelectedDestinationBrush);

            //    if (IsForePixelModeSelected)
            //    {
            //        // Call the SourceOver method from the model
            //        switch (SelectedCompositionMode)
            //        {
            //            case CompositionMode.SourceOver:
            //                Mat composedImage = compositeModeRendering.SourceOver(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath = SaveComposedImage(composedImage); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath); // Update the displayed image
            //                break;
            //            case CompositionMode.DestinationOver:
            //                Mat composedImage2 = compositeModeRendering.DestinationOver(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath2 = SaveComposedImage(composedImage2); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath2); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath2); // Update the displayed image
            //                break;
            //            case CompositionMode.SourceIn:
            //                Mat composedImage3 = compositeModeRendering.SourceIn(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath3 = SaveComposedImage(composedImage3); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath3); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath3); // Update the displayed image
            //                break;
            //            case CompositionMode.DestinationIn:
            //                Mat composedImage4 = compositeModeRendering.DestinationIn(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath4 = SaveComposedImage(composedImage4); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath4); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath4); // Update the displayed image
            //                break;
            //            case CompositionMode.SourceOut:
            //                Mat composedImage5 = compositeModeRendering.SourceOut(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath5 = SaveComposedImage(composedImage5); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath5); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath5); // Update the displayed image
            //                break;
            //            case CompositionMode.DestinationOut:
            //                Mat composedImage6 = compositeModeRendering.DestinationOut(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath6 = SaveComposedImage(composedImage6); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath6); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath6); // Update the displayed image
            //                break;
            //            case CompositionMode.SourceAtop:
            //                Mat composedImage7 = compositeModeRendering.SourceAtop(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath7 = SaveComposedImage(composedImage7); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath7); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath7); // Update the displayed image
            //                break;
            //            case CompositionMode.DestinationAtop:
            //                Mat composedImage8 = compositeModeRendering.DestinationAtop(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath8 = SaveComposedImage(composedImage8); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath8); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath8); // Update the displayed image
            //                break;
            //            case CompositionMode.Clear:
            //                Mat composedImage9 = compositeModeRendering.Clear(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath9 = SaveComposedImage(composedImage9); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath9); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath9); // Update the displayed image
            //                break;
            //            case CompositionMode.XOR:
            //                Mat composedImage10 = compositeModeRendering.XOR(tintedSourceImage, tintedDestinationImage);

            //                string composedImagePath10 = SaveComposedImage(composedImage10); // Save and get the file path
            //                ComposedImagePaths.Add(composedImagePath10); // Add to the collection
            //                UpdateDisplayedImage(composedImagePath10); // Update the displayed image
            //                break;
            //            // Add cases for other composition modes as needed
            //            default:
            //                break;
            //        }
            //    }
            //    if(_isForeMarksModeSelected)
            //    {
            //        // Glyph Rendering
            //        glyphRendering.PositiveGlyph = PositiveGlyph;
            //        glyphRendering.NegativeGlyph = NegativeGlyph;
            //        glyphRendering.NoDifferenceGlyph = NoDifferenceGlyph;

            //        Mat compositedFrame = glyphRendering.RenderDifferences(blurSourceImage, blurDestinationImage, AreaSize);

            //        string composedImagePath = SaveComposedImage(compositedFrame);
            //        ComposedImagePaths.Add(composedImagePath); // Add to the collection
            //        UpdateDisplayedImage(composedImagePath); // Update the displayed image
            //    }  
            }
        }

        private string SaveComposedImage(Mat composedImage)
        {
            string tempFolderPath = System.IO.Path.GetTempPath(); // Example temporary folder
            string composedImagePath = System.IO.Path.Combine(tempFolderPath, $"ComposedImage_{DateTime.Now:yyyyMMddHHmmss}.png");
            Cv2.ImWrite(composedImagePath, composedImage);
            return composedImagePath;
        }

        private void UpdateDisplayedImage(string imagePath)
        {
            CompositedImage = new ImageModel { ImagePath = imagePath };
            OnPropertyChanged(nameof(CompositedImage));
        }


        /// <summary>
        /// Applies a color tint to the given image using the specified brush color, ensuring 60% opacity.
        /// </summary>
        /// <param name="image">The input image to which the tint will be applied.</param>
        /// <param name="tintColorBrush">The brush containing the tint color.</param>
        /// <returns>A new image with the tint color applied and 60% opacity.</returns>
        /// <remarks>
        /// This method extracts the color values from the given brush and creates a Scalar
        /// object representing the tint color. It then applies the tint color to the input image
        /// by blending the input image with the tint color and sets the opacity to 60%.
        /// </remarks>
        private Mat ApplyColorTint(Mat image, SolidColorBrush tintColorBrush)
        {
            // Extract color values from the brush
            System.Windows.Media.Color tintColor = tintColorBrush.Color;
            Scalar tint = new Scalar(tintColor.B, tintColor.G, tintColor.R);

            // Separate the channels of the input image
            Mat[] channels = Cv2.Split(image);
            Mat alphaChannel = channels.Length == 4 ? channels[3] : Mat.Ones(image.Size(), MatType.CV_8UC1) * 255;

            // Create a new Mat for the tinted image
            Mat tintedRGB = new Mat();
            Cv2.Multiply(image, new Scalar(0.5, 0.5, 0.5, 1.0), tintedRGB); // Reduce the original image brightness by 50%
            Mat tintMat = new Mat(image.Size(), image.Type(), tint);
            Cv2.AddWeighted(tintedRGB, 1.0, tintMat, 0.5, 0, tintedRGB); // Apply the tint color

            // Create a new alpha channel with 60% opacity
            Mat alpha60 = new Mat(image.Size(), MatType.CV_8UC1, new Scalar(153)); // 60% of 255 is 153

            // Merge the tinted RGB channels with the new alpha channel
            if (channels.Length == 4)
            {
                Mat[] tintedChannels = Cv2.Split(tintedRGB);
                tintedChannels[3] = alpha60; // Set the new alpha channel
                Cv2.Merge(tintedChannels, tintedRGB);
            }
            else
            {
                // If the original image did not have an alpha channel, add the alpha channel
                Cv2.Merge(new Mat[] { tintedRGB, alpha60 }, tintedRGB);
            }

            return tintedRGB;
        }

        #region EventHandlers
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

            if (name == nameof(SelectedForegroundCompositionMode))
            {
                OnCompositionModeChanged(this, new PropertyChangedEventArgs(nameof(SelectedForegroundCompositionMode)));
            }
            if(name == nameof(SelectedBackgroundCompositionMode))
            {
                OnCompositionModeChanged(this, new PropertyChangedEventArgs(nameof(SelectedBackgroundCompositionMode)));
            }
        }

        // Event handler for composition mode changes
        private void OnCompositionModeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedForegroundCompositionMode) || e.PropertyName == nameof(SelectedBackgroundCompositionMode))
            {
                if (CompositedImage != null)
                {
                    CompositedImage.Clear(); // Clear CompositiedImage so the new Composite Image can be saved
                    OnPropertyChanged(nameof(CompositedImage));
                }

                ComposeImages();
            }
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

            if (selectedItems.Count == 1)
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

                        // Calculate Frame Offset value
                        OffsetValue = SelectedFrames[1].FrameNumber - SelectedFrames[0].FrameNumber;
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
        #endregion
    }
}
