using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using TemporalMotionExtractionAnalysis.Model;
using OpenCvSharp;
using System.Diagnostics;
using System.Windows.Media;
using System.Drawing;
using TemporalMotionExtractionAnalysis.Converters;
using System.Globalization;
using OpenCvSharp.Extensions;
using System.Drawing.Text;
using static TemporalMotionExtractionAnalysis.Model.MarkRendering;

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
        private ObservableCollection<string> _backgroundTextureTints;

        private ImageModel _previousImage;
        private ImageModel _currentImage;
        private ImageModel _indicationImage;
        private ImageModel _nextImage;
        private ImageModel _offsetFrame;
        private ImageModel _compositiedImage;

        private System.Windows.Media.Color _selectedSourceColor;
        private System.Windows.Media.Color _selectedDestinationColor;
        private System.Windows.Media.Color _selectedTextureColor;
        private SolidColorBrush _selectedSourceBrush;
        private SolidColorBrush _selectedDestinationBrush;

        private int _imageCount;
        private int _currentIndex;
        private int _offsetValue;
        private int _timeDelay;
        private int _selectedFps;
        private int _areaSize;
        private int _currentBlurSize;
        private int _offsetBlurSize;
        private int _currentFrameTransparency;
        private int _offsetFrameTransparency;

        private const int TotalCells = 120; //57
        private const int CenterIndex = 1; //26

        private double _calculatedEmeasure;
        private double _calculatedMAE;
        private double _calculatedSSIM;

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
        private string _selectedForegroundCompositionMode;
        private string _selectedBackgroundCompositionMode;
        private string _selectedBackgroundMarksTextures;
        private string _positiveGlyph = "▲";
        private string _negativeGlyph = "▼";
        private string _noDifferenceGlyph = "■";

        private ObservableCollection<string> composedImagePaths = new ObservableCollection<string>();
        private ObservableCollection<string> transformedCurrentImagePaths = new ObservableCollection<string>();
        private ObservableCollection<string> transformedOffsetImagePaths = new ObservableCollection<string>();

        private MarkRendering glyphRendering;

        private OpenCvSharp.Size _currentKernelSize;
        private OpenCvSharp.Size _offsetKernelSize;

        private RenderResult _renderResult;
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

        public System.Windows.Media.Color SelectedTextureColor
        {
            get => _selectedTextureColor;
            set
            {
                _selectedTextureColor = value;
                OnPropertyChanged(nameof(SelectedTextureColor));
                OnPropertyChanged(nameof(SelectedTextureBrush));
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

        public SolidColorBrush SelectedTextureBrush
        {
            get { return new SolidColorBrush(SelectedTextureColor); }
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

        public ObservableCollection<string> BackgroundMarksColors
        {
            get => _backgroundTextureTints;
            set
            {
                if (_backgroundTextureTints != value)
                {
                    _backgroundTextureTints = value;
                    OnPropertyChanged(nameof(_backgroundTextureTints));
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

        #region Strings
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

        public string FolderName
        {
            get => _folderName;
            set
            {
                _folderName = value;
                OnPropertyChanged(nameof(FolderName));
            }
        }

        public string NegativeMark
        {
            get => _negativeGlyph;
            set
            {
                _negativeGlyph = value;
                OnPropertyChanged(nameof(NegativeMark));
            }
        }

        public string NoDifferenceMark
        {
            get => _noDifferenceGlyph;
            set
            {
                _noDifferenceGlyph = value;
                OnPropertyChanged(nameof(NoDifferenceMark));
            }
        }

        public string PositiveMark
        {
            get => _positiveGlyph;
            set
            {
                _positiveGlyph = value;
                OnPropertyChanged(nameof(PositiveMark));
            }
        }
        #endregion

        #region Doubles
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
        #endregion

        #region Integers
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

        public int ImageCount
        {
            get { return _imageCount; }
            set
            {
                _imageCount = value;
                OnPropertyChanged(nameof(ImageCount));
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

        public int CurrentFrameTransparency
        {
            get => _currentFrameTransparency;
            set
            {
                _currentFrameTransparency = value;
                OnPropertyChanged(nameof(CurrentFrameTransparency)); // Notify property changed
            }
        }

        public int OffsetFrameTransparency
        {
            get => _offsetFrameTransparency;
            set
            {
                _offsetFrameTransparency = value;
                OnPropertyChanged(nameof(OffsetFrameTransparency)); // Notify property changed
            }
        }
        #endregion

        #region ImageModels
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

        public ImageModel IndicationImage
        {
            get => _indicationImage;
            set
            {
                _indicationImage = value;
                OnPropertyChanged(nameof(IndicationImage));
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
        #endregion

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

        #region OpenCvSharp.Size Variables
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

        public RenderResult RenderResult
        {
            get => _renderResult;
            set
            {
                _renderResult = value;
                OnPropertyChanged(nameof(RenderResult));
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
            
            SelectedFps = 4; // Default value is 4
            FolderName = "No Selected Folder";
            SelectedFrames = new ObservableCollection<ImageModel>();
            SelectedCurrentBlurSize = 7; // Default value is 7
            SelectedOffsetBlurSize = 7;
            AreaSize = 40; // Default value

            // String values for comboboxes in the View
            FpsValue = new ObservableCollection<string>() { "4", "10", "20", "30", "40", "60" };
            BlurValue = new ObservableCollection<string>() { "5", "7", "9" };
            SourceColors = new ObservableCollection<string>() { "none", "Red", "Yellow", "Green" };
            DestinationColors = new ObservableCollection<string>() { "none", "Orange", "Purple", "Blue" };
            CompositionModes =  new ObservableCollection<string>() { "SourceOver", "DestinationOver", "SourceIn", 
                "DestinationIn", "SourceOut", "DestinationOut", "SourceAtop", "DestinationAtop", "Clear", "XOR" };
            BackgroundMarksTextures = new ObservableCollection<string>() { "Crosshatch", "Double Helix", "Circle", "Plus", "Minus", "Slash", "Double Circle", "Dot", "Asterisk" };
            BackgroundMarksColors = new ObservableCollection<string>() { "none", "White", "LightSlateGray", "Silver", "DarkGray", "DimGray", "SlateGray" };

            // Create an instance of the StringToColorConverter
            var converter = new StringToColorConverter();

            // Initialize the Color values
            // Set the default selected value to the first item in the collection
            if (SourceColors.Any())
            {
                SelectedSourceColor = (System.Windows.Media.Color)converter.ConvertBack(SourceColors.First(), typeof(System.Windows.Media.Color), null, CultureInfo.InvariantCulture);
            }

            // Set the default selected value to the first item in the collection
            if (DestinationColors.Any())
            {
                SelectedDestinationColor = (System.Windows.Media.Color)converter.ConvertBack(DestinationColors.First(), typeof(System.Windows.Media.Color), null, CultureInfo.InvariantCulture);
            }

            // Set the default selected value to the first item in the collection
            if (BackgroundMarksColors.Any())
            {
                SelectedTextureColor = (System.Windows.Media.Color)converter.ConvertBack(BackgroundMarksColors.First(), typeof(System.Windows.Media.Color), null, CultureInfo.InvariantCulture);
            }

            // Initialize the ObservableCollection<ImageModel> for the Images
            Images = new ObservableCollection<ImageModel>();
            TimelineCells = new ObservableCollection<ImageModel>();
            ZoommedTimelineCells = new ObservableCollection<ImageModel>();

            // Default values for sliders
            CurrentFrameTransparency = 50;
            OffsetFrameTransparency = 50;

            // Radio button defaults
            IsForePixelModeSelected = true;
            IsBackPixelModeSelected = true;

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
            for (int i = 0; i < 9; i++)
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

            // Initialize the MarkRendering class with default glyphs
            glyphRendering = new MarkRendering("▲", "▼", "■");

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
            int centerCellIndex = 4; // Center cell is always at index 4

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
                ZoommedTimelineCells[i].IsCurrent = (i == centerCellIndex); // Center cell is always at index 4

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
                        IndicationImage = Images.First();
                    }

                    FolderName = System.IO.Path.GetFileName(selectedPath);

                    IsImagesLoaded = true;
                }
            }
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

        #region ImageProcessing
        // Method to update glyphs from the View
        public void UpdateGlyphs(string positive, string negative, string noDifference)
        {
            glyphRendering.StrongMotionMark = positive;
            glyphRendering.NegativeMark = negative;
            glyphRendering.NoMotionMark = noDifference;
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

        /// <summary>
        /// Processes frame transformations on an image, including optional XOR rendering, color inversion, alpha reduction, and blurring.
        /// </summary>
        /// <param name="frameImagePath">The file path of the image to be processed.</param>
        /// <param name="isReduceNoiseSelected">A boolean indicating whether XOR rendering should be applied.</param>
        /// <returns>The file path of the transformed image.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified image file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the image fails to load from the specified path.</exception>
        private Mat ProcessFrameTransforms(Mat frameImage, bool isReduceNoiseSelected)
        {
            // Motion Extraction of the current image
            MotionExtraction motionExtraction = new MotionExtraction();

            // Step 1: Invert
            Mat invertedImage = motionExtraction.InvertColors(frameImage);

            // Step 2: Apply Reduction of Noise (if selected)
            Mat transformedImage;
            if (isReduceNoiseSelected)
            {
                transformedImage = ReduceNoise(invertedImage);
            }
            else
            {
                transformedImage = invertedImage.Clone();
            }

            // Step 3: Reduce Alpha/Opacity
            Mat reducedAlphaCurrentImage = motionExtraction.ReduceAlpha(transformedImage, 0.5);

            // Step 4: Add Blur
            Mat blurCurrentImage = motionExtraction.BlurImage(reducedAlphaCurrentImage, CurrentKernelSize);
            string transformedCurrentImagePath = SaveComposedImage(blurCurrentImage); // Save and get the file path

            return blurCurrentImage;
        }


        /// <summary>
        /// Reduces background noise from an image using a series of image processing techniques.
        /// </summary>
        /// <param name="image">The input image from which to reduce noise.</param>
        /// <returns>A Mat object containing the processed image with reduced background noise.</returns>
        public Mat ReduceNoise(Mat image)
        {
            Mat result = new Mat();

            // Step 1: Convert the image to grayscale
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            // Step 2: Apply Gaussian Blur to reduce noise
            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(grayImage, blurredImage, new OpenCvSharp.Size(5, 5), 0);

            // Step 3: Apply adaptive thresholding to create a binary image
            Mat binaryImage = new Mat();
            Cv2.AdaptiveThreshold(blurredImage, binaryImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);

            // Step 4: Apply morphological operations to remove small noise
            Mat morphImage = new Mat();
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(binaryImage, morphImage, MorphTypes.Open, kernel);

            // Step 5: Bitwise AND with the original image to retain the actual colors in the foreground
            Mat colorForeground = new Mat();
            Cv2.BitwiseAnd(image, image, colorForeground, morphImage);

            return colorForeground;
        }

        // Composition logic based on the selected mode
        private void ComposeImages()
        {
            // Call the Composition Mode method from the model
            if (SelectedFrames != null && SelectedFrames.Count >= 2)
            {
                // Get the image paths for source and destination images
                string sourceImagePath = CurrentImage.ImagePath;
                string destinationImagePath = SelectedFrames[1].ImagePath;

                // Load the images from their paths
                Mat sourceImage = Cv2.ImRead(sourceImagePath);
                Mat destinationImage = Cv2.ImRead(destinationImagePath);

                // Motion Extraction of the sourceImage and destinationImage
                MotionExtraction motionExtraction = new MotionExtraction();
                CompositionModeRendering compositeModeRendering = new CompositionModeRendering();

                // Do any image pre-processing from user selections
                Mat processedSourceImage = ProcessFrameTransforms(sourceImage, IsCurrentXORSelected);
                Mat processedDestinationImage = ProcessFrameTransforms(destinationImage, IsOffsetXORSelected);

                // Create an instance of the StringToColorConverter
                var converter = new StringToColorConverter();

                // Create the Instance Mask - distinguish Foreground from Background
                Mat sourceFGMask = new Mat();
                Mat destinationFGMask = new Mat();
                Mat instanceMask = new Mat();
                (sourceFGMask, destinationFGMask, instanceMask) =  motionExtraction.InstanceMask(processedSourceImage, processedDestinationImage, SelectedSourceColor, SelectedDestinationColor);
                string savedSourceMask = SaveComposedImage(sourceFGMask);
                string savedDestMask = SaveComposedImage(destinationFGMask);
                string savedInstanceMask = SaveComposedImage(instanceMask);

                // Step 4: Tint the Source and Destination according to the user selections
                Mat tintedSourceImage = motionExtraction.ApplyTint(sourceImage, SelectedSourceColor);
                Mat tintedDestinationImage = motionExtraction.ApplyTint(destinationImage, SelectedDestinationColor);

                // RenderCompositeImage
                Mat composedImage = RenderCompositeImage(tintedSourceImage, tintedDestinationImage, sourceFGMask, destinationFGMask, instanceMask);
                string composedImagePath = SaveComposedImage(composedImage);
                ComposedImagePaths.Add(composedImagePath); // Add to the collection
                UpdateDisplayedImage(composedImagePath); // Update the displayed image

                // Display the overall average metrics of the result
                CalculatedEmeasure = Math.Round(motionExtraction.CalculateEmeasurePixelwise(sourceImage, destinationImage), 4);
                CalculatedMAE = Math.Round(motionExtraction.CalculateMAE(sourceImage, destinationImage), 4);
                CalculatedSSIM = Math.Round(motionExtraction.CalculateSSIM(sourceImage, destinationImage), 4);
            }
        }

        private Mat RenderCompositeImage(Mat sourceImage, Mat destinationImage, Mat sourceMask, Mat destinationMask, Mat instanceMask)
        {
            // Split the source and destination images into their channels
            Mat[] sourceChannels = sourceImage.Split();
            Mat[] destChannels = destinationImage.Split();
            Mat[] sourceMaskChannels = sourceMask.Split();
            Mat[] destMaskChannels = destinationMask.Split();

            // Use the alpha channels from the masks
            Mat sourceFGMask = sourceMaskChannels[3];
            Mat destinationFGMask = destMaskChannels[3];

            // Create background masks
            Mat sourceBGMask = new Mat();
            Mat destinationBGMask = new Mat();
            Cv2.BitwiseNot(sourceFGMask, sourceBGMask);
            Cv2.BitwiseNot(destinationFGMask, destinationBGMask);

            // Extract foreground and background parts (4-channel)
            Mat sourceForeground = new Mat();
            Mat sourceBackground = new Mat();
            Mat destForeground = new Mat();
            Mat destBackground = new Mat();

            Cv2.Merge(sourceChannels, sourceForeground);
            Cv2.Merge(sourceChannels, sourceBackground);

            Cv2.Merge(destChannels, destForeground);
            Cv2.Merge(destChannels, destBackground);

            sourceForeground = ApplyMask(sourceForeground, sourceFGMask);
            sourceBackground = ApplyMask(sourceBackground, sourceBGMask);

            destForeground = ApplyMask(destForeground, destinationFGMask);
            destBackground = ApplyMask(destBackground, destinationBGMask);

            // Save debug images
            sourceForeground.SaveImage("debug_images/debug_sourceFG_image" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            destForeground.SaveImage("debug_images/debug_destFG_image" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            sourceBackground.SaveImage("debug_images/debug_sourceBG_image" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            destBackground.SaveImage("debug_images/debug_destBG_image" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");

            Mat foregroundResult = new Mat(sourceImage.Height, sourceImage.Width, MatType.CV_8UC4, new Scalar(0, 0, 0, 128));
            Mat backgroundResult = new Mat();
            Mat result = new Mat();

            if (IsForePixelModeSelected)
            {
                CompositionModeRendering compositeModeRendering = new CompositionModeRendering();
                foregroundResult = RenderForeground(sourceForeground, destForeground, compositeModeRendering);                
                foregroundResult.SaveImage("debug_images/debug_FGresult" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            }
            else if (IsForeMarksModeSelected)
            {
                // Mark Rendering
                glyphRendering.StrongMotionMark = PositiveMark;
                glyphRendering.NegativeMark = NegativeMark;
                glyphRendering.NoMotionMark = NoDifferenceMark;
                foregroundResult = glyphRendering.RenderDifferences(sourceForeground, destForeground, AreaSize, instanceMask);
                foregroundResult.SaveImage("debug_images/debug_FGresult" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            }
            else
            {
                throw new InvalidOperationException("Neither Pixel nor Marks mode is selected.");
            }

            if (IsBackPixelModeSelected)
            {
                CompositionModeRendering compositionModeRendering = new CompositionModeRendering();
                backgroundResult = RenderForeground(sourceBackground, destBackground, compositionModeRendering);
                backgroundResult.SaveImage("debug_images/debug_BGresult" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
            }
            else if (IsBackMarksModeSelected)
            {
                backgroundResult = RenderBackground(sourceBackground, destBackground, instanceMask);
                backgroundResult.SaveImage("debug_images/debug_BGresult" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");

                backgroundResult = ApplyColorTint(backgroundResult, SelectedSourceBrush);
            }
            else
            {
                throw new InvalidOperationException("Neither Pixel nor Marks mode is selected.");
            }

            // Combine foreground and background
            Mat comboResult = CombineWithBackground(backgroundResult, sourceBackground);
            result = CombineWithBackground(foregroundResult, comboResult);

            // Clean up
            foreach (var channel in sourceChannels) channel.Dispose();
            foreach (var channel in destChannels) channel.Dispose();
            foreach (var channel in sourceMaskChannels) channel.Dispose();
            foreach (var channel in destMaskChannels) channel.Dispose();
            sourceFGMask.Dispose();
            sourceBGMask.Dispose();
            destinationFGMask.Dispose();
            destinationBGMask.Dispose();

            return result;
        }

        private Mat ApplyMask(Mat image, Mat mask)
        {
            Mat result = new Mat();
            Mat[] channels = image.Split();

            for (int i = 0; i < channels.Length; i++)
            {
                Cv2.Multiply(channels[i], mask, channels[i], 1.0 / 255.0);
            }

            // If the image doesn't have an alpha channel, add one
            if (channels.Length == 3)
            {
                Array.Resize(ref channels, 4);
                channels[3] = mask.Clone();
            }
            else if (channels.Length == 4)
            {
                // If it already has an alpha channel, multiply it with the mask
                Cv2.Multiply(channels[3], mask, channels[3], 1.0 / 255.0);
            }

            Cv2.Merge(channels, result);

            foreach (var channel in channels) channel.Dispose();

            return result;
        }

        /// <summary>
        /// Combines a foreground image with transparency over a background image without transparency.
        /// </summary>
        /// <param name="foreground">The foreground Mat with transparency (3 or 4 channels).</param>
        /// <param name="background">The background Mat without transparency (3 or 4 channels).</param>
        /// <returns>A new Mat with the foreground blended over the background, resulting in a 3-channel (BGR) image.</returns>
        /// <remarks>
        /// This function performs the following operations:
        /// 1. Ensures the background is in BGR format (3 channels).
        /// 2. Ensures the foreground is in BGRA format (4 channels) to utilize its alpha channel.
        /// 3. For each color channel (B, G, R):
        ///    - Multiplies the foreground by its alpha channel.
        ///    - Multiplies the background by the inverse of the foreground's alpha channel.
        ///    - Adds these results together to create the blended channel.
        /// 4. Merges the blended channels into a final 3-channel (BGR) image.
        /// 
        /// The resulting image will show the foreground overlaid on the background, with the 
        /// foreground's transparency determining how much of the background is visible.
        /// The final image is fully opaque (no alpha channel).
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the foreground and background are not the same size, or if either image
        /// doesn't have 3 or 4 channels.
        /// </exception>
        public Mat CombineWithBackground(Mat foreground, Mat background)
        {
            if (foreground.Size() != background.Size())
                throw new ArgumentException("Foreground and background must be the same size.");

            Mat result = new Mat();
            Mat fgra = new Mat();
            Mat bgra = new Mat();

            // Ensure background has 3 channels (BGR)
            if (background.Channels() == 3)
                bgra = background.Clone();
            else if (background.Channels() == 4)
                Cv2.CvtColor(background, bgra, ColorConversionCodes.BGRA2BGR);
            else
                throw new ArgumentException("Background must have 3 or 4 channels");

            // Ensure foreground has 4 channels (BGRA)
            if (foreground.Channels() == 3)
                Cv2.CvtColor(foreground, fgra, ColorConversionCodes.BGR2BGRA);
            else if (foreground.Channels() == 4)
                fgra = foreground.Clone();
            else
                throw new ArgumentException("Foreground must have 3 or 4 channels");

            // Split the foreground into channels
            Mat[] fgraSplit = fgra.Split();

            // Create the result channels
            Mat[] resultChannels = new Mat[3]; // Only 3 channels for BGR
            for (int i = 0; i < 3; i++)  // For BGR channels
            {
                resultChannels[i] = new Mat();

                // Blend foreground and background based on foreground alpha
                Mat fgChannel = new Mat();
                Mat bgChannel = new Mat();

                Cv2.Multiply(fgraSplit[i], fgraSplit[3], fgChannel, 1.0 / 255);
                Cv2.Multiply(bgra.ExtractChannel(i), new Mat(bgra.Size(), MatType.CV_8UC1, new Scalar(255)) - fgraSplit[3], bgChannel, 1.0 / 255);

                Cv2.Add(fgChannel, bgChannel, resultChannels[i]);
            }

            // Merge the channels to create the final result
            Cv2.Merge(resultChannels, result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="destinationImage"></param>
        /// <param name="compositeModeRendering"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private Mat RenderForeground(Mat sourceImage, Mat destinationImage, CompositionModeRendering compositeModeRendering)
        {
            switch (SelectedForegroundCompositionMode)
            {
                case "SourceOver":
                    return compositeModeRendering.SourceOver(sourceImage, destinationImage);
                case "DestinationOver":
                    return compositeModeRendering.DestinationOver(sourceImage, destinationImage);
                case "SourceIn":
                    return compositeModeRendering.SourceInBlend(sourceImage, destinationImage);
                case "DestinationIn":
                    return compositeModeRendering.DestinationIn(sourceImage, destinationImage);
                case "SourceOut":
                    return compositeModeRendering.SourceOut(sourceImage, destinationImage);
                case "DestinationOut":
                    return compositeModeRendering.DestinationOut(sourceImage, destinationImage);
                case "SourceAtop":
                    return compositeModeRendering.SourceAtop(sourceImage, destinationImage);
                case "DestinationAtop":
                    return compositeModeRendering.DestinationAtopBlend(sourceImage, destinationImage);
                case "Clear":
                    return compositeModeRendering.Clear(sourceImage, destinationImage);
                case "XOR":
                    return compositeModeRendering.XorBlend(sourceImage, destinationImage);
                default:
                    throw new ArgumentException("Invalid composition mode selected.", nameof(SelectedForegroundCompositionMode));
            }
        }

        private Mat RenderBackground( Mat sourceImageBackground, Mat destinationImageBackground, Mat mask)
        {
            // Create a result Mat of the same size and type as the original mask
            Mat result = new Mat(mask.Size(), mask.Type());
            mask.CopyTo(result);

            int areaSize = 20;

            Bitmap bitmap = new Bitmap(result.Width, result.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var textureColor = System.Drawing.Color.FromArgb(
                SelectedTextureColor.A, SelectedTextureColor.R, SelectedTextureColor.G, SelectedTextureColor.B);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                g.Clear(textureColor);

                Font font = new Font("Segoe UI", 20, System.Drawing.FontStyle.Regular); // Font 14 in Segoe UI

                for (int y = -areaSize; y < result.Rows; y += areaSize)
                {
                    for (int x = -areaSize; x < result.Cols; x += areaSize)
                    {
                        int checkX = Math.Max(0, x);
                        int checkY = Math.Max(0, y);

                        // Extract the window from mask to check for active areas
                        OpenCvSharp.Rect window = new OpenCvSharp.Rect(checkX, checkY,
                            Math.Min(areaSize, result.Cols - checkX),
                            Math.Min(areaSize, result.Rows - checkY));

                        // Convert the windowMat to grayscale
                        Mat windowMat = new Mat(result, window);
                        Cv2.CvtColor(windowMat, windowMat, ColorConversionCodes.BGR2GRAY);

                        // Calculate the percentage of black pixels in the window
                        double totalPixels = window.Width * window.Height; 
                        double blackPixels = totalPixels - Cv2.CountNonZero(windowMat);

                        // Debug: Print the number of blank pixels
                        Console.WriteLine($"Blank Pixels at ({x}, {y}): {blackPixels}");

                        if (blackPixels / totalPixels >= 0.01)// Check if 45% or more are black
                        {
                            if (x >= areaSize && y >= areaSize)
                            {
                                // Draw the gray X in the center of the window
                                PointF position = new PointF(x - (int)(0.5*areaSize), y - (int)(1.25*areaSize));

                                switch (SelectedBackgroundMarksTextures)
                                {
                                    case "Crosshatch":
                                        font = new Font("Segoe UI", 22, System.Drawing.FontStyle.Regular);
                                        g.DrawString("\u00D7", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Double Helix":
                                        font = new Font("Segoe UI", 22, System.Drawing.FontStyle.Regular);
                                        g.DrawString("X", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Slash":
                                        g.DrawString("/", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Plus":
                                        g.DrawString("+", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Minus":
                                        g.DrawString("-", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Circle":
                                        g.DrawString("○", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Double Circle":
                                        g.DrawString("⦾", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Dot":
                                        g.DrawString("•", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    case "Asterisk":
                                        font = new Font("Segoe UI", 30, System.Drawing.FontStyle.Regular);
                                        g.DrawString("*", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                    default:
                                        g.DrawString("X", font, System.Drawing.Brushes.Gray, position);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
                        
            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
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
        /// Applies a color tint to the given image using the specified Brush color, ensuring 60% opacity.
        /// </summary>
        /// <param name="image">The input image to which the tint will be applied.</param>
        /// <param name="tintColorBrush">The Brush containing the tint color.</param>
        /// <returns>A new image with the tint color applied and 60% opacity.</returns>
        /// <remarks>
        /// This method extracts the color values from the given Brush and creates a Scalar
        /// object representing the tint color. It then applies the tint color to the input image
        /// by blending the input image with the tint color and sets the opacity to 60%.
        private Mat ApplyColorTint(Mat image, SolidColorBrush tintColorBrush)
        {
            // Extract color values from the Brush
            System.Windows.Media.Color tintColor = tintColorBrush.Color;
            Scalar tint = new Scalar(tintColor.B, tintColor.G, tintColor.R);

            // Separate the channels of the input image
            Mat[] channels = Cv2.Split(image);
            Mat alphaChannel = channels.Length == 4 ? channels[3] : Mat.Ones(image.Size(), MatType.CV_8UC1) * 255;

            // Create a new Mat for the tinted image
            Mat tintedImage = new Mat();
            Cv2.Multiply(image, new Scalar(0.5, 0.5, 0.5, 1.0), tintedImage); // Reduce the original image brightness by 50%
            Mat tintMat = new Mat(image.Size(), image.Type(), tint);
            Cv2.AddWeighted(tintedImage, 0.5, tintMat, 0.5, 0, tintedImage); // Apply the tint color

            // Create a new alpha channel with 60% opacity
            Mat alpha60 = alphaChannel * 0.6; // 60% of the original alpha values

            if (channels.Length == 4)
            {
                // If the original image had an alpha channel, update the alpha channel
                channels[3] = alpha60;
                Cv2.Merge(channels, tintedImage);
            }
            else
            {
                // If the original image did not have an alpha channel, add the alpha channel
                Mat[] tintedChannels = Cv2.Split(tintedImage);
                Mat[] mergedChannels = new Mat[] { tintedChannels[0], tintedChannels[1], tintedChannels[2], alpha60 };
                Cv2.Merge(mergedChannels, tintedImage);
            }

            return tintedImage;
        }
        #endregion

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
            // Update the IsOffsetSelection property for TimelineCells
            foreach (var image in TimelineCells)
            {
                image.IsOffsetSelection = false; // Reset all selections
                foreach (var selectedItem in selectedItems)
                {
                    if (selectedItem is ImageModel selectedImage && image.FrameNumber == selectedImage.FrameNumber)
                    {
                        image.IsOffsetSelection = true;
                        break;
                    }
                }
            }

            // Update the IsOffsetSelection property for ZoomedTimelineCells
            foreach (var image in ZoommedTimelineCells)
            {
                image.IsOffsetSelection = false; // Reset all selections
                foreach (var selectedItem in selectedItems)
                {
                    if (selectedItem is ImageModel selectedImage && image.FrameNumber == selectedImage.FrameNumber)
                    {
                        image.IsOffsetSelection = true;
                        break;
                    }
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
                        var currentFrame = Images[CurrentIndex];
                        currentFrame.IsOffsetSelection = true;
                        SelectedFrames.Add(currentFrame);

                        // User Selected Offset Frame
                        imageModel.IsOffsetSelection = true;
                        SelectedFrames.Add(imageModel);

                        // Calculate Frame Offset value
                        OffsetValue = imageModel.FrameNumber - currentFrame.FrameNumber;
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

        /// <summary>
        /// Handles the changing of the indication image upon mouse over in Zoomed Timeline.
        /// </summary>
        /// <param name="image">The image that is moused over.</param>
        /// <remarks>
        /// </remarks>
        public void HandleIndicationSelectionChanged(ImageModel image)
        {
            IndicationImage = image;
        }


        #endregion
    }
}
