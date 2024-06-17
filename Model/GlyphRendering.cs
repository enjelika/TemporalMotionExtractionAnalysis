using System.Drawing;
using System.Drawing.Text;
using OpenCvSharp;
using OpenCvSharp.Extensions;

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

        public Mat RenderDifferences(Mat mat1, Mat mat2)
        {
            // Ensure both Mats have the same size and type
            if (mat1.Size() != mat2.Size() || mat1.Type() != mat2.Type())
                throw new ArgumentException("Mats must have the same size and type.");

            // Create a result Mat of the same size and type
            Mat result = new Mat(mat1.Size(), mat1.Type());
            mat1.CopyTo(result);

            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = new Font("Segoe UI Symbol", 32);
                Brush positiveBrush = Brushes.Green;
                Brush negativeBrush = Brushes.Red;
                Brush noDifferenceBrush = Brushes.Blue;

                for (int y = 0; y < mat1.Rows; y++)
                {
                    for (int x = 0; x < mat1.Cols; x++)
                    {
                        var diff = mat1.At<byte>(y, x) - mat2.At<byte>(y, x);

                        string glyph = null;
                        Brush brush = null;

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

                        g.DrawString(glyph, font, brush, x * 10, y * 10);
                    }
                }
            }

            // Convert the bitmap back to a Mat
            return BitmapConverter.ToMat(bitmap);
        }
    }
}