using System;
using System.Collections.Generic;
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

        // Size for the sliding window (AreaSize x AreaSize)
        public int AreaSize { get; set; }

        public GlyphRendering(string positiveMark, string negativeMark, string noDifferenceMark, int areaSize)
        {
            PositiveMark = positiveMark;
            NegativeMark = negativeMark;
            NoDifferenceMark = noDifferenceMark;
            AreaSize = areaSize;
        }

        public Mat RenderDifferences(Mat mat1, Mat mat2, int areaSize)
        {
            // Ensure both Mats have the same size and type
            if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
                throw new ArgumentException("Mats must have the same size and type.");

            // Calculate the difference between the two images
            Mat diffMat = mat2 - mat1;

            // Create a result Mat of the same size and type
            Mat result = new Mat(mat1.Size(), mat1.Type());
            mat1.CopyTo(result);

            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            List<(string glyph, Brush brush, PointF position)> glyphsToDraw = new List<(string, Brush, PointF)>();


            for (int y = 0; y <= diffMat.Rows-1; y += areaSize / 2)
            {
                for (int x = 0; x <= diffMat.Cols-1; x += areaSize / 2)
                {
                    // Define the window boundaries
                    int windowWidth = Math.Min(areaSize, diffMat.Cols - x);
                    int windowHeight = Math.Min(areaSize, diffMat.Rows - y);

                    // Extract the window from diffMat
                    Rect window = new Rect(x, y, windowWidth, windowHeight);
                    Mat windowMat = new Mat(diffMat, window);

                    // Calculate the average of the area within the window
                    Scalar avgScalar = Cv2.Mean(windowMat);
                    double avgValue = avgScalar.Val0;

                    // Determine the center of the window and apply the offset
                    float centerX = x + windowWidth / 2.0f - AreaSize / 2;
                    float centerY = y + windowHeight / 2.0f - AreaSize / 2;

                    // Determine the appropriate glyph and brush based on the average value
                    string glyph;
                    Brush brush;

                    if (avgValue > 0)
                    {
                        glyph = PositiveMark;
                        brush = Brushes.Green;
                    }
                    else if (avgValue < 0)
                    {
                        glyph = NegativeMark;
                        brush = Brushes.Red;
                    }
                    else
                    {
                        glyph = NoDifferenceMark;
                        brush = Brushes.Blue;
                    }

                    // Define the rectangle for the window
                    //Rectangle rect = new Rectangle(x, y, windowWidth, windowHeight);
                    glyphsToDraw.Add((glyph, brush, new PointF(centerX, centerY)));
                }
            }

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI Symbol", 12); // Reduced font size for efficiency

                foreach (var glyph in glyphsToDraw)
                {
                    // Draw the rectangle around the window
                    //g.DrawRectangle(Pens.Black, glyph.rect);
                    // Draw the glyph in the center of the window
                    g.DrawString(glyph.glyph, font, glyph.brush, glyph.position);
                }
            }

            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
        }
    }
}