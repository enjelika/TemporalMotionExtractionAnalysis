﻿using System.Collections.ObjectModel;
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
        private ImageModel _indicationImage;
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

        private GlyphRendering glyphRendering;

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
            BackgroundMarksTextures = new ObservableCollection<string>() { "Texture1", "Texture2", "Texture3" };

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
            SelectedForegroundCompositionMode = "SourceOver";

            // Initialize the GlyphRendering class with default glyphs
            glyphRendering = new GlyphRendering("▲", "▼", "■");

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
            glyphRendering.PositiveMark = positive;
            glyphRendering.NegativeMark = negative;
            glyphRendering.NoDifferenceMark = noDifference;
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
                Mat instanceMask = motionExtraction.InstanceMask(processedSourceImage, processedDestinationImage, SelectedSourceColor, SelectedDestinationColor);
                string savedInstanceMask = SaveComposedImage(instanceMask);

                // Convert Instance Mask to Binary Mask
                //Mat binaryMask = ConvertInstanceMaskToWhite(instanceMask);
                //string savedBinaryMask = SaveComposedImage(binaryMask);

                CalculatedEmeasure = Math.Round(motionExtraction.CalculateEmeasurePixelwise(sourceImage, destinationImage), 4);
                CalculatedMAE = Math.Round(motionExtraction.CalculateMAE(sourceImage, destinationImage), 4);
                CalculatedSSIM = Math.Round(motionExtraction.CalculateSSIM(sourceImage, destinationImage), 4);

                // Step 4: Tint the Source and Destination according to the user selections
                Mat tintedSourceImage = motionExtraction.ApplyTint(sourceImage, SelectedSourceColor);
                Mat tintedDestinationImage = motionExtraction.ApplyTint(destinationImage,SelectedDestinationColor);

                if (IsForePixelModeSelected)
                {
                    // Call the SourceOver method from the model
                    switch (SelectedForegroundCompositionMode)
                    {
                        case "SourceOver":
                            // Use the instanceMask to render the Foreground
                            Mat combinedImage = compositeModeRendering.SourceOver(tintedSourceImage, tintedDestinationImage);
                            //Mat foregroundResult = AddInstanceMasking(combinedImage, instanceMask);

                            // Use the instanceMask to render the Background
                            Mat finalImage = RenderBackground(instanceMask);

                            string composedImagePath = SaveComposedImage(finalImage); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath); // Add to the collection
                            UpdateDisplayedImage(composedImagePath); // Update the displayed image
                            break;
                        case "DestinationOver":
                            Mat composedImage2 = compositeModeRendering.DestinationOver(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath2 = SaveComposedImage(composedImage2); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath2); // Add to the collection
                            UpdateDisplayedImage(composedImagePath2); // Update the displayed image
                            break;
                        case "SourceIn":
                            Mat composedImage3 = compositeModeRendering.SourceIn(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath3 = SaveComposedImage(composedImage3); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath3); // Add to the collection
                            UpdateDisplayedImage(composedImagePath3); // Update the displayed image
                            break;
                        case "DestinationIn":
                            Mat composedImage4 = compositeModeRendering.DestinationIn(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath4 = SaveComposedImage(composedImage4); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath4); // Add to the collection
                            UpdateDisplayedImage(composedImagePath4); // Update the displayed image
                            break;
                        case "SourceOut":
                            Mat composedImage5 = compositeModeRendering.SourceOut(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath5 = SaveComposedImage(composedImage5); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath5); // Add to the collection
                            UpdateDisplayedImage(composedImagePath5); // Update the displayed image
                            break;
                        case "DestinationOut":
                            Mat composedImage6 = compositeModeRendering.DestinationOut(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath6 = SaveComposedImage(composedImage6); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath6); // Add to the collection
                            UpdateDisplayedImage(composedImagePath6); // Update the displayed image
                            break;
                        case "SourceAtop":
                            Mat composedImage7 = compositeModeRendering.SourceAtop(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath7 = SaveComposedImage(composedImage7); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath7); // Add to the collection
                            UpdateDisplayedImage(composedImagePath7); // Update the displayed image
                            break;
                        case "DestinationAtop":
                            Mat composedImage8 = compositeModeRendering.DestinationAtop(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath8 = SaveComposedImage(composedImage8); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath8); // Add to the collection
                            UpdateDisplayedImage(composedImagePath8); // Update the displayed image
                            break;
                        case "Clear":
                            Mat composedImage9 = compositeModeRendering.Clear(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath9 = SaveComposedImage(composedImage9); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath9); // Add to the collection
                            UpdateDisplayedImage(composedImagePath9); // Update the displayed image
                            break;
                        case "XOR":
                            Mat composedImage10 = compositeModeRendering.XOR(tintedSourceImage, tintedDestinationImage);

                            string composedImagePath10 = SaveComposedImage(composedImage10); // Save and get the file path
                            ComposedImagePaths.Add(composedImagePath10); // Add to the collection
                            UpdateDisplayedImage(composedImagePath10); // Update the displayed image
                            break;
                        // Add cases for other composition modes as needed
                        default:
                            break;
                    }
                }
                if (_isForeMarksModeSelected)
                {
                    // Glyph Rendering
                    glyphRendering.PositiveMark = PositiveMark;
                    glyphRendering.NegativeMark = NegativeMark;
                    glyphRendering.NoDifferenceMark = NoDifferenceMark;

                    Mat compositedFrame = glyphRendering.RenderDifferences(tintedSourceImage, tintedDestinationImage, AreaSize);

                    string composedImagePath = SaveComposedImage(compositedFrame);
                    ComposedImagePaths.Add(composedImagePath); // Add to the collection
                    UpdateDisplayedImage(composedImagePath); // Update the displayed image
                }
            }
        }

        static Mat ConvertInstanceMaskToWhite(Mat instanceMask)
        {
            // Create an output mask with the same size as the instance mask, initialized with all black pixels
            Mat whiteMask = new Mat(instanceMask.Size(), MatType.CV_8UC1, new Scalar(0)); // Create a black mask

            // Iterate through each pixel in the instance mask
            for (int y = 0; y < instanceMask.Rows; y++)
            {
                for (int x = 0; x < instanceMask.Cols; x++)
                {
                    Vec3b pixel = instanceMask.At<Vec3b>(y, x); // Get the pixel value

                    // Check if the pixel is not black (i.e., not (0, 0, 0))
                    if (pixel.Item0 != 0 || pixel.Item1 != 0 || pixel.Item2 != 0)
                    {
                        whiteMask.Set(y, x, new Scalar(255)); // Set non-black pixels to white in the output mask
                    }
                }
            }

            return whiteMask;
        }

        private Mat RenderBackground(Mat mask)
        {
            // Convert the mask to grayscale (assuming it has an alpha channel)
            Mat maskGray = new Mat();
            Cv2.CvtColor(mask, maskGray, ColorConversionCodes.BGRA2GRAY);

            // Threshold the grayscale mask to create a binary mask
            Mat binaryMask = new Mat();
            Cv2.Threshold(maskGray, binaryMask, 1, 255, ThresholdTypes.Binary);

            // Create a result Mat of the same size and type as the original mask
            Mat result = new Mat(mask.Size(), mask.Type());
            mask.CopyTo(result);

            int areaSize = 50;

            Bitmap bitmap = BitmapConverter.ToBitmap(result);

            // Patch holes with 
            //Bitmap bitmap = PatchHolesAndDrawX(bitmap0);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI", 16, System.Drawing.FontStyle.Regular); // Font 14 in Segoe UI

                for (int y = 0; y <= result.Rows - 1; y += areaSize / 2)
                {
                    for (int x = 0; x <= result.Cols - 1; x += areaSize / 2)
                    {
                        // Define the window boundaries
                        int windowWidth = Math.Min(areaSize, result.Cols - x);
                        int windowHeight = Math.Min(areaSize, result.Rows - y);

                        // Extract the window from xorImage to check for active areas
                        OpenCvSharp.Rect window = new OpenCvSharp.Rect(x, y, windowWidth, windowHeight);
                        Mat xorWindowMat = new Mat(mask, window);
                                                
                        // Convert the windowMat to grayscale
                        Mat windowMat = new Mat(result, window);
                        Cv2.CvtColor(windowMat, windowMat, ColorConversionCodes.BGR2GRAY);

                        // Calculate the percentage of black pixels in the window
                        double totalPixels = windowWidth * windowHeight;
                        double blackPixels = totalPixels - Cv2.CountNonZero(windowMat);

                        // Debug: Print the number of blank pixels
                        Console.WriteLine($"Blank Pixels at ({x}, {y}): {blackPixels}");

                        if (blackPixels / totalPixels >= 0.75) // Check if 75% or more are black
                        {
                            // Determine the center of the window and apply the offset
                            float centerX = x + windowWidth / 2.0f - areaSize / 2;
                            float centerY = y + windowHeight / 2.0f - areaSize / 2;

                            // Draw the gray X in the center of the window
                            PointF position = new PointF(centerX, centerY);
                            g.DrawString("X", font, System.Drawing.Brushes.Gray, position);
                        }
                        
                    }
                }
            }

            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
        }

        public Bitmap PatchHolesAndDrawX(Bitmap inputBitmap)
        {
            Mat inputMat = BitmapConverter.ToMat(inputBitmap);

            // Convert to grayscale
            Mat grayMat = new Mat();
            Cv2.CvtColor(inputMat, grayMat, ColorConversionCodes.BGRA2GRAY);

            // Threshold to create a binary mask of holes
            Mat binaryMask = new Mat();
            Cv2.Threshold(grayMat, binaryMask, 1, 255, ThresholdTypes.BinaryInv);

            // Fill holes using morphological closing
            Mat filledMask = new Mat();
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(binaryMask, filledMask, MorphTypes.Close, kernel);

            // Draw gray "X" in patched areas
            Bitmap resultBitmap = BitmapConverter.ToBitmap(inputMat);
            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI", 14, System.Drawing.FontStyle.Regular); // Font 14 in Segoe UI

                for (int y = 0; y < filledMask.Rows; y++)
                {
                    for (int x = 0; x < filledMask.Cols; x++)
                    {
                        if (filledMask.Get<byte>(y, x) > 0)
                        {
                            // Draw black square
                            int squareSize = 40; // Size of the square
                            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x - squareSize / 2, y - squareSize / 2, squareSize, squareSize);
                            g.FillRectangle(System.Drawing.Brushes.Black, rect);
                        }
                    }
                }
            }

            return resultBitmap;
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
        private Mat ApplyColorTint(Mat image, SolidColorBrush tintColorBrush)
        {
            // Extract color values from the brush
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
