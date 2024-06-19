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

        public GlyphRendering(string positiveGlyph, string negativeGlyph, string noDifferenceGlyph)
        {
            PositiveGlyph = positiveGlyph;
            NegativeGlyph = negativeGlyph;
            NoDifferenceGlyph = noDifferenceGlyph;
        }

        public Mat RenderDifferences(Mat mat1, Mat mat2)
        {
            // Perform XOR operation between the current image and the offset image
            Mat xorImage = new Mat();
            Cv2.BitwiseXor(currentImage, offsetImage, xorImage);

            return xorImage;
        }


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