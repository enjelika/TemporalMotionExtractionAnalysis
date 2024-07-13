using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemporalMotionExtractionAnalysis.Model
{
    internal class CompositionModeRendering
    {
        public CompositionModeRendering()
        {

        }

        /// <summary>
        /// Composes two images using the Porter-Duff Source Over composition mode.
        /// </summary>
        /// <param name="source">The source image to be composited.</param>
        /// <param name="destination">The destination image where the source image is composited onto.</param>
        /// <returns>The composited image using the Source Over composition mode.</returns>
        public Mat SourceOver(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha + dstAlpha * (1 - srcAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        double blended = (srcColor[c] * srcAlpha + dstColor[c] * dstAlpha * (1 - srcAlpha)) / outAlpha;
                        outColor[c] = (byte)Math.Round(blended);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Destination Over composition mode.
        /// </summary>
        /// <param name="source">The source image to be composited.</param>
        /// <param name="destination">The destination image that is composited under the source image.</param>
        /// <returns>The composited image using the Destination Over composition mode.</returns>
        public Mat DestinationOver(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = dstAlpha + srcAlpha * (1 - dstAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        double blended = (dstColor[c] * dstAlpha + srcColor[c] * srcAlpha * (1 - dstAlpha)) / outAlpha;
                        outColor[c] = (byte)Math.Round(blended);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Source In composition mode.
        /// The result shows the source image only where it overlaps with the destination image.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Source In composition mode.</returns>
        public Mat SourceIn(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha * dstAlpha;

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        outColor[c] = (byte)Math.Round(srcColor[c] * outAlpha);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Destination In composition mode.
        /// The result shows the destination image only where it overlaps with the source image.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Destination In composition mode.</returns>
        public Mat DestinationIn(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha * dstAlpha;

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        outColor[c] = (byte)Math.Round(dstColor[c] * outAlpha);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Source Out composition mode.
        /// The result shows the source image only where it doesn't overlap with the destination image.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Source Out composition mode.</returns>
        public Mat SourceOut(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha * (1 - dstAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        outColor[c] = (byte)Math.Round(srcColor[c] * outAlpha);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Destination Out composition mode.
        /// The result shows the destination image only where it doesn't overlap with the source image.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Destination Out composition mode.</returns>
        public Mat DestinationOut(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = dstAlpha * (1 - srcAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        outColor[c] = (byte)Math.Round(dstColor[c] * outAlpha);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Source Atop composition mode.
        /// The result shows the source image on top of the destination image, but only where the destination is opaque.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Source Atop composition mode.</returns>
        public Mat SourceAtop(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha * dstAlpha + dstAlpha * (1 - srcAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        double blended = (srcColor[c] * srcAlpha * dstAlpha + dstColor[c] * dstAlpha * (1 - srcAlpha)) / outAlpha;
                        outColor[c] = (byte)Math.Round(blended);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff Destination Atop composition mode.
        /// The result shows the destination image on top of the source image, but only where the source is opaque.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the Destination Atop composition mode.</returns>
        public Mat DestinationAtop(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = dstAlpha * srcAlpha + srcAlpha * (1 - dstAlpha);

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        double blended = (dstColor[c] * dstAlpha * srcAlpha + srcColor[c] * srcAlpha * (1 - dstAlpha)) / outAlpha;
                        outColor[c] = (byte)Math.Round(blended);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }

        /// <summary>
        /// Composes a blank/clear image using the Porter-Duff Clear composition mode.
        /// The resulting image is fully transparent (clear) regardless of the input images.
        /// </summary>
        /// <param name="source">The source image (not used in this operation).</param>
        /// <param name="destination">The destination image (not used in this operation).</param>
        /// <returns>A fully transparent image of the same size as the input images.</returns>
        public Mat Clear(Mat source, Mat destination)
        {
            // Ensure both images have the same size
            if (source.Size() != destination.Size())
            {
                throw new ArgumentException("Both images must have the same size.");
            }

            // Create a new Mat with the same size as the input images, filled with zeros (fully transparent)
            Mat composition = new Mat(source.Size(), MatType.CV_8UC4, new Scalar(0, 0, 0, 0));

            return composition;
        }

        /// <summary>
        /// Composes two images using the Porter-Duff XOR composition mode.
        /// The result shows areas where either the source or destination is opaque, but not both.
        /// </summary>
        /// <param name="source">The source image for the composition.</param>
        /// <param name="destination">The destination image for the composition.</param>
        /// <returns>The composited image using the XOR composition mode.</returns>
        public Mat XOR(Mat source, Mat destination)
        {
            // Ensure both images have the same size and 4 channels (BGRA)
            if (source.Size() != destination.Size() || source.Channels() != 4 || destination.Channels() != 4)
            {
                throw new ArgumentException("Both images must have the same size and 4 channels (BGRA).");
            }

            int width = source.Cols;
            int height = source.Rows;
            Mat composition = new Mat(height, width, MatType.CV_8UC4);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b srcColor = source.Get<Vec4b>(y, x);
                    Vec4b dstColor = destination.Get<Vec4b>(y, x);

                    // Normalize alpha values to 0-1 range
                    double srcAlpha = srcColor[3] / 255.0;
                    double dstAlpha = dstColor[3] / 255.0;

                    // Calculate resulting alpha
                    double outAlpha = srcAlpha + dstAlpha - 2 * srcAlpha * dstAlpha;

                    Vec4b outColor = new Vec4b();
                    for (int c = 0; c < 3; c++) // For each color channel (BGR)
                    {
                        double blended = (srcColor[c] * srcAlpha * (1 - dstAlpha) + dstColor[c] * dstAlpha * (1 - srcAlpha)) / outAlpha;
                        outColor[c] = (byte)Math.Round(blended);
                    }
                    outColor[3] = (byte)Math.Round(outAlpha * 255); // Convert alpha back to 0-255 range

                    composition.Set(y, x, outColor);
                }
            }

            return composition;
        }
    }
}
