using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using TemporalMotionExtractionAnalysis.Model;

namespace TemporalMotionExtractionAnalysis.Controls
{
    public class SSIMImageControl : Image
    {
        private RenderResult _renderResult;
        private ToolTip _toolTip = new ToolTip();

        public SSIMImageControl()
        {
            ToolTip = _toolTip;
            MouseMove += SSIMImageControl_MouseMove;
        }

        public void SetRenderResult(RenderResult result)
        {
            _renderResult = result;
            Source = BitmapToImageSource(result.RenderedImage);
        }

        private BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void SSIMImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_renderResult != null)
            {
                var position = e.GetPosition(this);
                var glyph = _renderResult.MarkData.FirstOrDefault(g =>
                    Math.Abs(g.Position.X - position.X) < g.FontSize / 2 &&
                    Math.Abs(g.Position.Y - position.Y) < g.FontSize / 2);

                if (glyph != null)
                {
                    string brushColor = "Unknown";
                    if (glyph.Brush == System.Drawing.Brushes.Blue) brushColor = "Blue";
                    else if (glyph.Brush == System.Drawing.Brushes.Red) brushColor = "Red";
                    else if (glyph.Brush == System.Drawing.Brushes.Yellow) brushColor = "Yellow";
                    else if (glyph.Brush == System.Drawing.Brushes.Green) brushColor = "Green";

                    _toolTip.Content = $"SSIM: {glyph.SSIM:F4}, Color: {brushColor}";
                }
                else
                {
                    _toolTip.Content = "";
                }
            }
        }
    }
}