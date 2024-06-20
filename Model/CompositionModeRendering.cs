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
        /// Composes two images using the Source Over composition mode.
        /// Source image masks destination image in composition image.
        /// Source pixel is displayed if not black.
        /// </summary>
        /// <param name="source">The source image to be composited.</param>
        /// <param name="destination">The destination image where the source image is composited onto.</param>
        /// <returns>The composited image using the Source Over composition mode.</returns>
        public Mat SourceOver(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // Source image masks destination image in composition image.
                    // Source pixel is displayed if not black.
                    if (sourceColor.Item0 > 100 && sourceColor.Item3 > 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes two images using the Destination Over composition mode.
        /// Destination image masks source destination image in composition image.
        /// Destination pixel is displayed if not black.
        /// </summary>
        /// <param name="source">The source image to be composited.</param>
        /// <param name="destination">The destination image taht is composited onto the source image.</param>
        /// <returns>The composited image using the Destination Over composition mode.</returns>
        public Mat DestinationOver(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // Destination image masks source destination image in composition image.
                    // Destination pixel is displayed if not black.
                    if (destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes an intersection of two images using the Source In composition mode.
        /// The intersection of source and destination images is displayed as the source color.
        /// </summary>
        /// <param name="source">The source image for the intersction composit.</param>
        /// <param name="destination">The destination image for the intersction composit.</param>
        /// <returns>The composited image using the Source In composition mode.</returns>
        public Mat SourceIn(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The intersection of source and destination images 
                    // is displayed as the source color.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else
                    {
                        destinationColor.Item0 = 0;
                        destinationColor.Item1 = 0;
                        destinationColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes an intersection of two images using the Destination In composition mode.
        /// The intersection of source and destination images is displayed as the destination color.
        /// </summary>
        /// <param name="source">The source image for the intersection composit.</param>
        /// <param name="destination">The destination image for the intersection composit.</param>
        /// <returns>The composited image using the Destination In composition mode.</returns>
        public Mat DestinationIn(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The intersection of source and destination images 
                    // is displayed as the destination color.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else
                    {
                        sourceColor.Item0 = 0;
                        sourceColor.Item1 = 0;
                        sourceColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes an area of exclusion of two images using the Source Out composition mode.
        /// The intersection of source and destination images is displayed as the source color.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the Source Out composition mode.</returns>
        public Mat SourceOut(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The intersection of source and destination images 
                    // is displayed as the source color.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 < 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else
                    {
                        destinationColor.Item0 = 0;
                        destinationColor.Item1 = 0;
                        destinationColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes an area of exclusion of two images using the Destination Out composition mode.
        /// The area of the destination image not intersecting the source image is displayed as 
        /// the destination color.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the Destination Out composition mode.</returns>
        public Mat DestinationOut(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The area of the destination image not intersecting
                    // the source image is displayed as the destination color.
                    if (sourceColor.Item0 < 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else
                    {
                        sourceColor.Item0 = 0;
                        sourceColor.Item1 = 0;
                        sourceColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes areas of both intersection and exclusion of two images using the SourceAtop composition mode.
        /// The intersection of source and destination images is displayed as the source color and the remaining 
        /// destination area is displayed also.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the SourceAtop composition mode.</returns>
        public Mat SourceAtop(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The intersection of source and destination images 
                    // is displayed as the source color and the remaining destination
                    // area is displayed also.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else if (sourceColor.Item0 < 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else
                    {
                        sourceColor.Item0 = 0;
                        sourceColor.Item1 = 0;
                        sourceColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes areas of both intersection and exclusion of two images using the DestinationAtop composition mode.
        /// The intersection of source and destination images is displayed as the destination color and the remaining 
        /// source area is displayed also.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the DestinationAtop composition mode.</returns>
        public Mat DestinationAtop(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The intersection of source and destination images 
                    // is displayed as the destination color and the remaining source
                    // area is displayed also.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else if (sourceColor.Item0 > 100 && destinationColor.Item2 < 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else
                    {
                        sourceColor.Item0 = 0;
                        sourceColor.Item1 = 0;
                        sourceColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }
            return composition;
        }

        /// <summary>
        /// Composes a blank/clear image using the Clear composition mode.
        /// The composition image is blank/clear.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the Clear composition mode.</returns>
        public Mat Clear(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    sourceColor.Item0 = 0;
                    sourceColor.Item1 = 0;
                    sourceColor.Item2 = 0;
                    sourceColor.Item3 = 255;
                    composition.Set<Vec4b>(i, j, sourceColor);
                }
            }
            return composition;
        }


        /// <summary>
        /// Composes an image of mutual exclusion using the XOR composition mode.
        /// The areas of source and destination images that
        /// are mutually exclusive are each displayed.
        /// </summary>
        /// <param name="source">The source image for the composit.</param>
        /// <param name="destination">The destination image for the composit.</param>
        /// <returns>The composited image using the XOR composition mode.</returns>
        public Mat XOR(Mat source, Mat destination)
        {
            // Get pixel data
            int width = source.Width;
            int height = source.Height;

            // Instantiate composition matrix
            Mat composition = new Mat(height, width, MatType.CV_8UC3);

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Create composition image from source and destination images                    
                    Vec4b sourceColor = source.At<Vec4b>(i, j);
                    Vec4b destinationColor = destination.At<Vec4b>(i, j);

                    // The areas of source and destination images that
                    // are mutually exclusive are each displayed.
                    if (sourceColor.Item0 > 100 && destinationColor.Item2 < 100)
                    {
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                    else if (sourceColor.Item0 < 100 && destinationColor.Item2 > 100)
                    {
                        composition.Set<Vec4b>(i, j, destinationColor);
                    }
                    else
                    {
                        sourceColor.Item0 = 0;
                        sourceColor.Item1 = 0;
                        sourceColor.Item2 = 0;
                        composition.Set<Vec4b>(i, j, sourceColor);
                    }
                }
            }

            return composition;

            //if (source.Size() != destination.Size() || source.Type() != destination.Type())
            //{
            //    throw new ArgumentException("Source and destination Mats must have the same size and type.");
            //}

            //Mat sourceAlpha = new Mat();
            //Mat destinationAlpha = new Mat();
            //Cv2.ExtractChannel(source, sourceAlpha, 3); // Extract alpha channel from source
            //Cv2.ExtractChannel(destination, destinationAlpha, 3); // Extract alpha channel from destination

            //Mat xorAlpha = new Mat();
            //Cv2.BitwiseXor(sourceAlpha, destinationAlpha, xorAlpha); // XOR the alpha channels

            //Mat result = new Mat();
            //Cv2.AddWeighted(source, 1, destination, 1, 0, result); // Combine source and destination images
            //Cv2.InsertChannel(xorAlpha, result, 3); // Insert the XORed alpha channel into the result

            //return result;
        }
    }
}
