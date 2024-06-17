using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemporalMotionExtractionAnalysis.Model
{
    internal class GlyphRendering
    {
        // Glyphs for positive, negative, and no difference areas
        public string PositiveGlyph { get; set; }
        public string NegativeGlyph { get; set; }
        public string NoDifferenceGlyph { get; set; }

        public GlyphRendering(string positiveGlyph, string negativeGlyph, string noDifferenceGlyph)
        {
            PositiveGlyph = positiveGlyph;
            NegativeGlyph = negativeGlyph;
            NoDifferenceGlyph = noDifferenceGlyph;
        }

        // Function to compare two Mats and render the results
        public Mat RenderDifferences(Mat mat1, Mat mat2)
        {
            if (mat1.Size() != mat2.Size())
                throw new System.ArgumentException("The input Mat objects must have the same size.");

            Mat result = new Mat(mat1.Size(), MatType.CV_8UC3, new Scalar(255, 255, 255)); // White background

            for (int y = 0; y < mat1.Rows; y++)
            {
                for (int x = 0; x < mat1.Cols; x++)
                {
                    Vec3b color1 = mat1.At<Vec3b>(y, x);
                    Vec3b color2 = mat2.At<Vec3b>(y, x);

                    string glyph = GetGlyph(color1, color2);
                    RenderGlyph(result, glyph, x, y);
                }
            }

            return result;
        }

        private string GetGlyph(Vec3b color1, Vec3b color2)
        {
            int intensity1 = (color1.Item0 + color1.Item1 + color1.Item2) / 3;
            int intensity2 = (color2.Item0 + color2.Item1 + color2.Item2) / 3;

            if (intensity1 > intensity2)
                return PositiveGlyph;
            else if (intensity1 < intensity2)
                return NegativeGlyph;
            else
                return NoDifferenceGlyph;
        }

        private void RenderGlyph(Mat image, string glyph, int x, int y)
        {
            int fontFace = (int)HersheyFonts.HersheySimplex;
            double fontScale = 0.5;
            int thickness = 1;

            Cv2.PutText(image, glyph, new Point(x * 10, y * 10), (HersheyFonts)fontFace, fontScale, new Scalar(0, 0, 0), thickness); // Black text
        }
    }
}
