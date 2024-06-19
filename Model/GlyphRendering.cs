using System.Drawing;
using System.Drawing.Text;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace TemporalMotionExtractionAnalysis.Model
{
    internal class GlyphRendering
    {
        // Glyphs for positive, negative, and no difference areas
        public string PositiveMark { get; set; }
        public string NegativeMark { get; set; }
        public string NoDifferenceMark { get; set; }

        public string BackgroundMark { get; set; }

        public GlyphRendering(string positiveGlyph, string negativeGlyph, string noDifferenceGlyph)
        {
            PositiveMark = positiveGlyph;
            NegativeMark = negativeGlyph;
            NoDifferenceMark = noDifferenceGlyph;

            // Define BackgroundMark as the Segoe UI Symbol "&#xE10A;"
            BackgroundMark = "\xE10A"; // Unicode escape sequence for '&#xE10A;'
        }

        public Mat XorImages(Mat currentImage, Mat offsetImage)
        {
            // Perform XOR operation between the current image and the offset image
            Mat xorImage = new Mat();
            Cv2.BitwiseXor(currentImage, offsetImage, xorImage);

            return xorImage;
        }

        public Mat RenderMarksResults(Mat currentImage, Mat offsetImage, int areaSize)
        {
            // Ensure both Mats have the same size and type
            if (currentImage.Size() != offsetImage.Size() || currentImage.Type() != offsetImage.Type())
                throw new ArgumentException("Mats must have the same size and type.");

            // Calculate the difference between the offsetImage and currentImage
            Mat diffMat = offsetImage - currentImage;

            // Create a foreground mask by thresholding the absolute difference
            Mat foregroundMask = new Mat();
            Cv2.Threshold(diffMat.Abs(), foregroundMask, 1, 255, ThresholdTypes.Binary);

            // Create a background mask by bitwise XOR of foreground mask and full white image
            Mat backgroundMask = new Mat(currentImage.Size(), MatType.CV_8UC1, new Scalar(255));
            Cv2.BitwiseXor(backgroundMask, foregroundMask, backgroundMask);

            // Create a result Mat as a copy of the currentImage
            Mat emptyMat = new Mat(currentImage.Size(), currentImage.Type());
            currentImage.CopyTo(emptyMat);
            Bitmap result = BitmapConverter.ToBitmap(emptyMat);
            
            // Initialize a list to store mark information
            List<(string mark, System.Drawing.Brush brush, PointF position)> marksToDraw = new List<(string, System.Drawing.Brush, PointF)>();

            // Perform sliding window over the differences
            for (int y = 0; y <= diffMat.Rows - 1; y += areaSize / 2)
            {
                for (int x = 0; x <= diffMat.Cols - 1; x += areaSize / 2)
                {
                    // Define the window boundaries
                    int windowWidth = Math.Min(areaSize, diffMat.Cols - x);
                    int windowHeight = Math.Min(areaSize, diffMat.Rows - y);
                    Rect window = new Rect(x, y, windowWidth, windowHeight);

                    // Extract the window from diffMat
                    Mat windowMat = new Mat(diffMat, window);

                    // Calculate the average value within the windowMat
                    Scalar avgScalar = Cv2.Mean(windowMat);
                    double avgValue = avgScalar.Val0;

                    // Determine the appropriate mark based on the average value
                    string mark;
                    System.Drawing.Brush brush;

                    if (avgValue > 0)
                    {
                        mark = PositiveMark;
                        brush = System.Drawing.Brushes.Green;
                    }
                    else if (avgValue < 0)
                    {
                        mark = NegativeMark;
                        brush = System.Drawing.Brushes.Red;
                    }
                    else
                    {
                        mark = NoDifferenceMark;
                        brush = System.Drawing.Brushes.Blue;
                    }

                    // Calculate the center of the window
                    int centerX = x + windowWidth / 2;
                    int centerY = y + windowHeight / 2;

                    // Save the mark information
                    marksToDraw.Add((mark, brush, new PointF(centerX, centerY)));
                }
            }

            // Render the marks on the result Mat based on foreground and background masks
            foreach (var (x, y, mark) in marksToDraw)
            {
                // Determine if the mark should be rendered on foreground or background
                Mat mask = foregroundMask.Get<byte>(y, x) > 0 ? foregroundMask : backgroundMask;

                // Check if the mask value at (x, y) is greater than 0
                if (mask.Get<byte>(y, x) > 0)
                {
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.TextRenderingHint = TextRenderingHint.AntiAlias;
                        Font font = new Font("Segoe UI Symbol", 13);
                        foreach (var currentMark in marksToDraw)
                        {
                            g.DrawString(mark, font, brush, mark.position);
                        }
                    }
                }
            }

            return result;
        }


        public Mat RenderDifferences(Mat currentImage, Mat offsetImage, int areaSize)
        {
            if (currentImage.Size() != offsetImage.Size() || currentImage.Type() != offsetImage.Type())
                throw new ArgumentException("Mats must have the same size and type.");
            Mat xorImage = XorImages(currentImage, offsetImage);
            Mat diffMat = offsetImage - currentImage;
            Mat xorMar = new Mat();
            Cv2.Threshold(diffMat, xorMar, 1, 255, ThresholdTypes.Binary); // where is this being saved?
            Mat result = new Mat(currentImage.Size(), currentImage.Type());
            currentImage.CopyTo(result);
            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            List<(string glyph, Brush brush, PointF position)> glyphsToDraw = new List<(string, Brush, PointF)>();
            for (int y = 0; y <= diffMat.Rows - 1; y += areaSize / 2)
            {
                for (int x = 0; x <= diffMat.Cols - 1; x += areaSize / 2)
                {
                    int windowWidth = Math.Min(areaSize, diffMat.Cols - x);
                    int windowHeight = Math.Min(areaSize, diffMat.Rows - y);
                    Rect window = new Rect(x, y, windowWidth, windowHeight);
                    Mat xorWindowMat = new Mat(xorImage, window);
                    Mat windowMat = new Mat(diffMat, window);
                    // Print sizes and types for debugging
                    Console.WriteLine("windowMat Size: " + windowMat.Size());
                    Console.WriteLine("windowMat Type: " + windowMat.Type());
                    Console.WriteLine("xorMar Size: " + xorMar.Size());
                    Console.WriteLine("xorMar Type: " + xorMar.Type());
                    Mat foregroundMask = new Mat();
                    Cv2.BitwiseAnd(windowMat, xorMar, foregroundMask);
                    Mat backgroundMask = new Mat();
                    Cv2.BitwiseXor(xorMar, foregroundMask, backgroundMask);
                    Scalar sumForeground = Cv2.Sum(foregroundMask);
                    Scalar sumBackground = Cv2.Sum(backgroundMask);
                    // Calculate the average value within the windowMat
                    Scalar avgScalar = Cv2.Mean(windowMat);
                    double avgValue = avgScalar.Val0;
                    // Determine the appropriate mark and brush based on the average value
                    string mark;
                    Brush brush;
                    float centerX = x + windowWidth / 2.0f - areaSize / 2;
                    float centerY = y + windowHeight / 2.0f - areaSize / 2;
                    if (sumForeground.Val0 > 0) // Foreground areas of interest
                    {
                        if (avgValue > 0)
                        {
                            mark = PositiveMark;
                            brush = Brushes.Green;
                        }
                        else if (avgValue < 0)
                        {
                            mark = NegativeMark;
                            brush = Brushes.Red;
                        }
                        else
                        {
                            mark = NoDifferenceMark;
                            brush = System.Drawing.Brushes.Blue;
                        }
                        glyphsToDraw.Add((mark, brush, new PointF(centerX, centerY)));
                    }
                    else if (sumBackground.Val0 > 0) // Background areas
                    {
                        // Determine appropriate mark and brush for background areas
                        mark = BackgroundMark;
                        brush = Brushes.Gray;
                        glyphsToDraw.Add((mark, brush, new PointF(centerX, centerY)));
                    }
                }
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI Symbol", 13);
                foreach (var glyph in glyphsToDraw)
                {
                    g.DrawString(glyph.glyph, font, glyph.brush, glyph.position);
                }
            }
            return BitmapConverter.ToMat(bitmap);
        }

        /*
        public Mat RenderDifferences(Mat currentImage, Mat offsetImage, int areaSize)
        {
            // Ensure both Mats have the same size and type
            if (currentImage.Size() != offsetImage.Size() || currentImage.Type() != offsetImage.Type())
                throw new ArgumentException("Mats must have the same size and type.");

            // Perform XOR operation between the images to get the active areas
            Mat xorImage = XorImages(currentImage, offsetImage);

            // Calculate the difference between the two images
            Mat diffMat = offsetImage - currentImage;

            // Create a result Mat of the same size and type
            Mat result = new Mat(currentImage.Size(), currentImage.Type());
            currentImage.CopyTo(result);

            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            List<(string mark, Brush brush, PointF position)> glyphsToDraw = new List<(string, Brush, PointF)>();

            for (int y = 0; y <= diffMat.Rows - 1; y += areaSize / 2)
            {
                for (int x = 0; x <= diffMat.Cols - 1; x += areaSize / 2)
                {
                    // Define the window boundaries
                    int windowWidth = Math.Min(areaSize, diffMat.Cols - x);
                    int windowHeight = Math.Min(areaSize, diffMat.Rows - y);

                    // Extract the window from xorImage to check for active areas
                    Rect window = new Rect(x, y, windowWidth, windowHeight);
                    Mat xorWindowMat = new Mat(xorImage, window);

                    // Check if there are active areas in the xorWindowMat
                    Scalar sumXor = Cv2.Sum(xorWindowMat);
                    if (sumXor.Val0 == 0) // Skip non-active areas
                    {
                        continue;
                    }

                    // Extract the window from diffMat
                    Mat windowMat = new Mat(diffMat, window);

                    // Calculate the average of the area within the window
                    Scalar avgScalar = Cv2.Mean(windowMat);
                    double avgValue = avgScalar.Val0;

                    // Determine the center of the window and apply the offset
                    float centerX = x + windowWidth / 2.0f - areaSize / 2;
                    float centerY = y + windowHeight / 2.0f - areaSize / 2;

                    // Determine the appropriate mark and brush based on the average value
                    string mark;
                    Brush brush;

                        if (diff > 0)
                        {
                            mark = PositiveGlyph;
                            brush = positiveBrush;
                        }
                        else if (diff < 0)
                        {
                            mark = NegativeGlyph;
                            brush = negativeBrush;
                        }
                        else
                        {
                            mark = NoDifferenceGlyph;
                            brush = noDifferenceBrush;
                        }

                    glyphsToDraw.Add((mark, brush, new PointF(centerX, centerY)));
                }
            }

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI Symbol", 13); // Reduced font size for efficiency

                foreach (var mark in glyphsToDraw)
                {
                    g.DrawString(mark.mark, font, mark.brush, mark.position);
                }
            }

            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
        }
        */
    }
}