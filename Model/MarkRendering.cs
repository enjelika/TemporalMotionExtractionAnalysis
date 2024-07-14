using System.Drawing;
using System.Drawing.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace TemporalMotionExtractionAnalysis.Model
{
    public class RenderResult
    {
        public Bitmap RenderedImage { get; set; }
        public List<MarkData> MarkData { get; set; }
    }

    public class MarkData
    {
        public string Mark { get; set; }
        public Brush Brush { get; set; }
        public PointF Position { get; set; }
        public float FontSize { get; set; }
        public double SSIM { get; set; }
    }

    internal class MarkRendering
    {
        // Glyphs for positive, negative, and no difference areas
        public string StrongMotionMark { get; set; }
        public string SlightMotionMark { get; set; }
        public string NegativeMark { get; set; }
        public string NoMotionMark { get; set; }

        public MarkRendering(string positiveMark, string negativeMark, string noDifferenceMark)
        {
            StrongMotionMark = positiveMark;
            NegativeMark = negativeMark;
            NoMotionMark = noDifferenceMark;
        }
            
        /// <summary>
        /// Renders the differences between two images using SSIM-based comparison
        /// </summary>
        /// <param name="currentImage">The current image Mat</param>
        /// <param name="offsetImage">The offset image Mat for comparison</param>
        /// <param name="areaSize">The size of the sliding window area</param>
        /// <returns>Mat with rendered difference marks</returns>
        /// <exception cref="ArgumentException"></exception>
        public Mat RenderDifferences(Mat currentImage, Mat offsetImage, int areaSize, Mat instanceMask)
        {
            if (currentImage.Size() != offsetImage.Size() || currentImage.Type() != offsetImage.Type() || currentImage.Size() != instanceMask.Size())
                throw new ArgumentException("All Mats must have the same size and type.");

            Mat result = currentImage.Clone();
            // Create a bitmap with transparency
            Bitmap bitmap = new Bitmap(currentImage.Width, currentImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            List<MarkData> glyphsToDraw = new List<MarkData>();

            // Convert mask to grayscale if it's not already
            Mat maskGray = instanceMask.Channels() == 1 ? instanceMask.Clone() : new Mat();
            if (instanceMask.Channels() > 1)
                Cv2.CvtColor(instanceMask, maskGray, ColorConversionCodes.BGR2GRAY);

            for (int y = 0; y < currentImage.Rows; y += areaSize)
            {
                for (int x = 0; x < currentImage.Cols; x += areaSize)
                {
                    int windowWidth = Math.Min(areaSize, currentImage.Cols - x);
                    int windowHeight = Math.Min(areaSize, currentImage.Rows - y);

                    Rect window = new Rect(x, y, windowWidth, windowHeight);

                    // Check if the window intersects with any non-zero (active) area in the mask
                    Mat maskWindow = new Mat(maskGray, window);
                    if (Cv2.CountNonZero(maskWindow) == 0) continue; // Skip if window is not in active area

                    Mat currentWindow = new Mat(currentImage, window);
                    Mat offsetWindow = new Mat(offsetImage, window);

                    // Calculate SSIM for the window
                    double ssim = CalculateSSIM(currentWindow, offsetWindow);

                    float centerX = x + windowWidth / 2.0f;
                    float centerY = y + windowHeight / 2.0f;

                    string mark;
                    Brush brush;

                    // Adjusted thresholds for camouflaged animal detection
                    if (ssim > 0.99) // Very high similarity - likely no motion
                    {
                        mark = NoMotionMark;
                        brush = Brushes.Blue;
                    }
                    else if (ssim > 0.97) // High similarity - possible slight motion, could be camouflaged animal
                    {
                        mark = NegativeMark;
                        brush = Brushes.Red;
                    }
                    else if (ssim > 0.95) // Moderate similarity - more noticeable motion
                    {
                        mark = "•"; // Small dot for subtle changes
                        brush = Brushes.Yellow;
                    }
                    else // Lower similarity - significant motion
                    {
                        mark = StrongMotionMark;
                        brush = Brushes.Green;
                    }

                    // Determine font size based on areaSize
                    float fontSize = (areaSize >= 0 && areaSize <= 39) ? 20 :
                             (areaSize >= 40 && areaSize <= 59) ? 40 :
                             (areaSize >= 60 && areaSize <= 80) ? 60 :
                             (areaSize >= 81 && areaSize <= 100) ? 80 : 95;

                    glyphsToDraw.Add(new MarkData
                    {
                        Mark = mark,
                        Brush = brush,
                        Position = new PointF(centerX, centerY),
                        FontSize = fontSize,
                        SSIM = ssim
                    });
                }
            }

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent); // Set the entire bitmap to transparent
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                foreach (var glyph in glyphsToDraw)
                {
                    using (Font font = new Font("Segoe UI Symbol", glyph.FontSize))
                    {
                        SizeF stringSize = g.MeasureString(glyph.Mark, font);
                        PointF adjustedPosition = new PointF(
                            glyph.Position.X - stringSize.Width / 2,
                            glyph.Position.Y - stringSize.Height / 2
                        );
                        g.DrawString(glyph.Mark, font, glyph.Brush, adjustedPosition);
                    }
                }
            }

            return BitmapConverter.ToMat(bitmap);
        }

        /// <summary>
        /// Calculates the Structural Similarity Index (SSIM) between two image windows
        /// </summary>
        /// <param name="img1">First image window</param>
        /// <param name="img2">Second image window for comparison</param>
        /// <returns>SSIM value between -1 and 1, where 1 indicates perfect similarity</returns>
        private double CalculateSSIM(Mat img1, Mat img2)
        {
            const double C1 = 6.5025, C2 = 58.5225;

            Mat I1 = new Mat(), I2 = new Mat();
            img1.ConvertTo(I1, MatType.CV_32F);
            img2.ConvertTo(I2, MatType.CV_32F);

            Mat I1_2 = I1.Mul(I1);
            Mat I2_2 = I2.Mul(I2);
            Mat I1_I2 = I1.Mul(I2);

            Mat mu1 = new Mat(), mu2 = new Mat();
            Cv2.GaussianBlur(I1, mu1, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(I2, mu2, new OpenCvSharp.Size(11, 11), 1.5);

            Mat mu1_2 = mu1.Mul(mu1);
            Mat mu2_2 = mu2.Mul(mu2);
            Mat mu1_mu2 = mu1.Mul(mu2);

            Mat sigma1_2 = new Mat(), sigma2_2 = new Mat(), sigma12 = new Mat();

            Cv2.GaussianBlur(I1_2, sigma1_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma1_2 -= mu1_2;

            Cv2.GaussianBlur(I2_2, sigma2_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma2_2 -= mu2_2;

            Cv2.GaussianBlur(I1_I2, sigma12, new OpenCvSharp.Size(11, 11), 1.5);
            sigma12 -= mu1_mu2;

            Mat t1 = new Mat(), t2 = new Mat(), t3 = new Mat();

            // t1 = (2 * mu1_mu2 + C1)
            Cv2.Multiply(mu1_mu2, 2, t1);
            Cv2.Add(t1, new Scalar(C1), t1);

            // t2 = (2 * sigma12 + C2)
            Cv2.Multiply(sigma12, 2, t2);
            Cv2.Add(t2, new Scalar(C2), t2);

            // t3 = t1.Mul(t2)
            t3 = t1.Mul(t2);

            // t1 = (mu1_2 + mu2_2 + C1)
            Cv2.Add(mu1_2, mu2_2, t1);
            Cv2.Add(t1, new Scalar(C1), t1);

            // t2 = (sigma1_2 + sigma2_2 + C2)
            Cv2.Add(sigma1_2, sigma2_2, t2);
            Cv2.Add(t2, new Scalar(C2), t2);

            // t1 = t1.Mul(t2)
            t1 = t1.Mul(t2);

            Mat ssim_map = new Mat();
            Cv2.Divide(t3, t1, ssim_map);

            Scalar mssim = Cv2.Mean(ssim_map);

            return mssim.Val0;
        }
    }
}