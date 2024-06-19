using System.Drawing;
using System.Drawing.Text;
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
                            brush = Brushes.Blue;
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
            List<(string glyph, Brush brush, PointF position)> glyphsToDraw = new List<(string, Brush, PointF)>();

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
                            glyph = PositiveGlyph;
                            brush = positiveBrush;
                        }
                        else if (diff < 0)
                        {
                            glyph = NegativeGlyph;
                            brush = negativeBrush;
                        }
                        else
                        {
                            glyph = NoDifferenceGlyph;
                            brush = noDifferenceBrush;
                        }

                    glyphsToDraw.Add((mark, brush, new PointF(centerX, centerY)));
                }
            }

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI Symbol", 13); // Reduced font size for efficiency

                foreach (var glyph in glyphsToDraw)
                {
                    g.DrawString(glyph.glyph, font, glyph.brush, glyph.position);
                }
            }

            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
        }
        */
    }
}