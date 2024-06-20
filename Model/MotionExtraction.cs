using System.IO;
using MOIE = Microsoft.Office.Interop.Excel;
using OpenCvSharp;
using OpenCvSharp.Quality;
using System.Windows.Xps.Packaging;
using System.Windows.Media;

namespace TemporalMotionExtractionAnalysis.Model
{
    internal class MotionExtraction
    {
        public MotionExtraction()
        {

        }

        #region MotionExtraction Image Processing Steps
        /// <summary>
        /// Inverts the colors of the input image.
        /// </summary>
        /// <param name="inputImage">The input Mat image whose colors need to be inverted.</param>
        /// <returns>
        /// A new Mat object containing the inverted image.
        /// </returns>
        /// <remarks>
        /// This function creates a new Mat object to store the inverted image. 
        /// It uses the OpenCV function Cv2.BitwiseNot to invert the colors of the input image.
        /// </remarks>
        /// <example>
        /// <code>
        /// Mat inputImage = Cv2.ImRead("path_to_image.jpg");
        /// Mat invertedImage = InvertColors(inputImage);
        /// Cv2.ImWrite("path_to_save_inverted_image.jpg", invertedImage);
        /// </code>
        /// This example demonstrates how to use the InvertColors function to invert the colors of an image and save the result.
        /// </example>
        public Mat InvertColors(Mat inputImage)
        {
            // Create a new Mat to store the inverted image
            Mat invertedImage = new Mat();

            // Invert the colors of the input image
            Cv2.BitwiseNot(inputImage, invertedImage);

            return invertedImage;
        }

        /// <summary>
        /// Reduces the alpha/opacity of an image, preparing it to be a mask for motion enhancement.
        /// </summary>
        /// <param name="image">The input image as a Mat object.</param>
        /// <returns>A modified image with reduced alpha/opacity.</returns>
        /// <remarks>
        /// This method converts the input image to grayscale, converts it to RGBA mode if necessary,
        /// and then reduces the opacity based on pixel intensity.
        /// </remarks>
        public Mat ReduceAlpha(Mat image, double transparencyValue)
        {
            Mat result = image.Clone();
            Mat[] channels = Cv2.Split(result);
            if (channels.Length == 4)
            {
                Mat alpha = channels[3];
                alpha *= transparencyValue;
                channels[3] = alpha;
                Cv2.Merge(channels, result);
            }
            return result;
        }


        /// <summary>
        /// Applies Gaussian blur to a given image Mat.
        /// </summary>
        /// <param name="sourceImage">The source image Mat to blur.</param>
        /// <param name="kernelSize">The size of the Gaussian kernel for blurring.</param>
        /// <returns>The blurred image as a Mat.</returns>
        public Mat BlurImage(Mat sourceImage, Size kernelSize)
        {
            if (sourceImage == null)
            {
                throw new ArgumentNullException(nameof(sourceImage), "Source image cannot be null.");
            }

            // Apply Gaussian blur
            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(sourceImage, blurredImage, kernelSize, sigmaX: 0, sigmaY: 0);

            return blurredImage;
        }
        #endregion

        #region Temporal Motion Analysis Metrics
        /// <summary>
        /// Calculates the Mean Absolute Error (MAE) between two image masks.
        /// </summary>
        /// <param name="prev_mask">The Mat of the previous image mask.</param>
        /// <param name="curr_mask">The Mat of the current image mask.</param>
        /// <returns>The Mean Absolute Error (MAE) between the two masks as a double value.</returns>
        public double CalculateMAE(Mat prev_mask, Mat curr_mask)
        {
            // Compute the absolute difference between the two masks
            Mat absolute_diff = Cv2.Abs(prev_mask - curr_mask);

            // Calculate the mean absolute error (MAE)
            double mae = (double)(Cv2.Mean(absolute_diff));
            return mae;
        }

        /// <summary>
        /// Calculates the mean E-measure pixelwise between two frames.
        /// </summary>
        /// <param name="prev_frame">The first frame (Mat object) in grayscale.</param>
        /// <param name="curr_frame">The second frame (Mat object) in grayscale.</param>
        /// <param name="threshold">The threshold for gradient difference (default: 10).</param>
        /// <returns>The mean E-measure value.</returns>
        public double CalculateEmeasurePixelwise(Mat prev_frame, Mat curr_frame, double threshold = 10)
        {
            // Compute the absolute pixel-wise difference between frames
            Mat prev_mask_f32 = new Mat();
            prev_frame.ConvertTo(prev_mask_f32, MatType.CV_32F);
            Mat curr_mask_f32 = new Mat();
            curr_frame.ConvertTo(curr_mask_f32, MatType.CV_32F);
            Mat absolute_diff = Cv2.Abs(prev_mask_f32 - curr_mask_f32);

            // Compute precision and recall based on the threshold
            double precision = (double)Cv2.Mean(absolute_diff.LessThan(threshold));
            double recall = (double)Cv2.Mean(curr_frame.LessThan(threshold));
            // Compute E-measure
            double alpha = 0.5;
            double e_measure = 1 - alpha * (1 - precision) - (1 - alpha) * (1 - recall);

            return e_measure;
        }

        /// <summary>
        /// Calculates the structural similarity index (SSIM) between two sequential frames.
        /// </summary>
        /// <param name="prev_frame">The first frame (Mat object) in grayscale.</param>
        /// <param name="curr_frame">The second frame (Mat object) in grayscale.</param>
        /// <returns>The SSIM score.</returns>
        public double CalculateSSIM(Mat prev_frame, Mat curr_frame)
        {
            // Calculate SSIM between two frames
            double score;
            using (var ssim = QualitySSIM.Create(prev_frame))
            {
                var result = ssim.Compute(curr_frame);
                score = result.Val0; // SSIM score
            }
            return score;
        }

        /// <summary>
        /// Calculates the Structural Similarity Index (SSIM) for each block of the given size in two input images
        /// and returns a matrix of SSIM values.
        /// </summary>
        /// <param name="prev_frame">The first input image (previous frame).</param>
        /// <param name="curr_frame">The second input image (current frame).</param>
        /// <param name="blockSize">The size of the blocks to use for SSIM calculation. Default is 8x8.</param>
        /// <returns>A Mat object containing SSIM values for each block in the input images.</returns>
        /// <exception cref="ArgumentException">Thrown when the input images have different sizes or types.</exception>
        /// 
        /// <remarks>
        /// The function divides the input images into blocks of the specified size, calculates the SSIM
        /// for each block, and stores the SSIM values in a matrix. The SSIM values provide a measure
        /// of similarity between corresponding blocks in the two images, with higher values indicating
        /// greater similarity.
        /// 
        /// The `CalculateSSIM` helper function computes the SSIM value for a given pair of blocks.
        /// 
        /// Example usage:
        /// <code>
        /// Mat prevFrame = Cv2.ImRead("prev_frame.png");
        /// Mat currFrame = Cv2.ImRead("curr_frame.png");
        /// Mat ssimMatrix = CalculateSSIMMatrix(prevFrame, currFrame, 8);
        /// </code>
        /// </remarks>
        public Mat CalculateSSIMMatrix(Mat prev_frame, Mat curr_frame, int blockSize)
        {
            // Ensure both frames have the same size and type
            if (prev_frame.Size() != curr_frame.Size() || prev_frame.Type() != curr_frame.Type())
            {
                throw new ArgumentException("Source and destination Mats must have the same size and type.");
            }

            // Get the width and height of the frames
            int width = prev_frame.Width;
            int height = prev_frame.Height;

            // Initialize the SSIM matrix to store SSIM values
            Mat ssimMatrix = new Mat(height / blockSize, width / blockSize, MatType.CV_64FC1, new Scalar(0));

            // Iterate through the image with the block size
            for (int y = 0; y < height; y += blockSize)
            {
                for (int x = 0; x < width; x += blockSize)
                {
                    // Define the region of interest (ROI) for each block
                    Rect roi = new Rect(x, y, Math.Min(blockSize, width - x), Math.Min(blockSize, height - y));
                    Mat prevBlock = new Mat(prev_frame, roi);
                    Mat currBlock = new Mat(curr_frame, roi);

                    // Calculate SSIM for the current block
                    double ssimValue = CalculateSSIM(prevBlock, currBlock);

                    // Set the SSIM value in the SSIM matrix
                    ssimMatrix.Set(y / blockSize, x / blockSize, ssimValue);
                }
            }

            return ssimMatrix;
        }
        #endregion

        public void MotionExtractionToExcel(string selectedPath)
        {
            //if __name__ == "__main__":
            //    # Load the workbook outside the loop
            //    # Create a workbook and add a worksheet.
            //    workbook = xlsxwriter.Workbook('PIL_Motion_Analysis.xlsx')
            //    worksheet = workbook.add_worksheet()
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            MOIE.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            MOIE.Worksheet worksheet = (MOIE.Worksheet)workbook.Worksheets.Add();

            //    # Add a bold format to use to highlight cells.
            //    bold = workbook.add_format({ 'bold': 1})
            MOIE.Range range = worksheet.get_Range("A1", "D1");
            range.Cells.Font.Bold = true;

            //    # Write some data headers.
            //    worksheet.write('A1', 'Folder', bold)
            //    worksheet.write('B1', 'Avg. MAE', bold)
            //    worksheet.write('C1', 'Avg. Em', bold)
            //    worksheet.write('D1', 'Avg. SSIM', bold)
            var cell = ((MOIE.Range)worksheet.Cells[1, 1]).Value = "Folder";
            cell = ((MOIE.Range)worksheet.Cells[1, 2]).Value = "Avg. MAE";
            cell = ((MOIE.Range)worksheet.Cells[1, 3]).Value = "Avg. Em";
            cell = ((MOIE.Range)worksheet.Cells[1, 4]).Value = "Avg. SSIM";

            //    # Get the number of folders in the JPEGImages directory
            //    num_folders = sum(os.path.isdir(os.path.join(jpeg_images_folder, item)) for item in os.listdir(jpeg_images_folder))
            //    print("Number of Folders: " + str(num_folders))
            string jpeg_images_folder = selectedPath; 
            var directories = Directory.GetDirectories(jpeg_images_folder);
            int num_folders = directories.Length;
            Console.WriteLine("Number of Folders: " + num_folders.ToString());
            //    # Start from the first cell below the headers.
            //    row = 1
            //    col = 0
            int row = 2;
            int col = 1;

            //    # Counter
            //    counter = 0
            int counter = 0;

            //    # Iterate through each folder in the JPEGImages directory
            DirectoryInfo directoryInfo = new DirectoryInfo(jpeg_images_folder);
            //    for folder in os.scandir(jpeg_images_folder):


            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                //        folder_path = folder.path  # Use folder.path to get the full path
                string folder_path = directory.FullName;
                //        # print(folder_path)

                //        # Check if the item in JPEGImages folder is a directory
                //        if os.path.isdir(folder_path):
                if (Directory.Exists(folder_path))
                {
                    //            # Get folder name for image collection
                    //            folder_name = folder.name
                    string folder_name = directory.Name;
                    //            print("Counter at: " + str(counter) + " with " + folder_name)
                    Console.WriteLine("Counter at: " + counter.ToString() + " with " + folder_name);
                    //            #if counter <= 9:
                    //              # counter += 1
                    //              # continue

                    //              # Your existing logic for processing frames
                    //            if counter < 2: #num_folders:   
                    //if (counter < 2)
                    //{
                    //#if counter == 5: #8:
                    //# extract_frames(gif_root + "/"+ file_name + ".gif", "./extracted_gif_frames/" + file_name)

                    //#if is_folder_empty("./extracted_gif_frames/" + file_name) and folder_has_multiple_files("./extracted_gif_frames/" + file_name):
                    //# extract_frames(gif_root + "/"+ file_name + ".gif", "./extracted_gif_frames/" + file_name)

                    //                average_MAE, average_Em, average_SSIM = motion_extraction(folder_path, folder_name, output_folder="./output_masks")
                    (double average_MAE, double average_Em, double average_SSIM) averages; // = motion_extraction(folder_path, folder_name, output_folder = "output_masks");
                    //                json_file_path = f"./results/{folder_name}_metrics_results.json"
                    string json_file_path = "results\\" + folder_name + "_metrics_results.json";
                    //                output_masks_path = f"./output_masks/{folder_name}"
                    string output_masks_path = "output_masks\\" + folder_name;
                    //                results_visualization(json_file_path, output_masks_path, folder_name)
                    //results_visualization(json_file_path, output_masks_path, folder_name);
                    //# make_gif(folder_name, folder_name)
                    //make_gif(folder_name, folder_name);
                    //# Row Data:    Folder_name     Avg. MAE    Avg. Em     Avg. SSIM
                    //worksheet.write_string  (  row, col,      folder_name  )
                    //worksheet.write_number  (  row, col + 1,  average_MAE  )
                    //worksheet.write_number  (  row, col + 2,  average_Em   )
                    //worksheet.write_number  (  row, col + 3,  average_SSIM )
                    //row += 1
                    ((MOIE.Range)worksheet.Cells[row, col]).Value = folder_name;
                    //((MOIE.Range)worksheet.Cells[row, col + 1]).Value = averages.average_MAE;
                    //((MOIE.Range)worksheet.Cells[row, col + 2]).Value = averages.average_Em;
                    //((MOIE.Range)worksheet.Cells[row, col + 3]).Value = averages.average_SSIM;
                    //# combine_gif_frames_for_pub_figure("./corgis_treadmill_motion_results.gif")
                    //                counter += 1
                    row += 1;
                    counter += 1;
                    //}
                    //else
                    //    break;
                }

            }
            // Save the updated workbook
            workbook.Close();
            Console.WriteLine("Spreadsheet with results saved.");
        }

        #region InstanceMask
        public Mat InstanceMask(Mat source, Mat destination)
        {
            Mat tintedSource = ApplyTint(source, Colors.Red);
            Mat tintedDestination0 = ApplyTint(destination, Colors.Red);

            // Create binary masks using color thresholding
            Mat redMask = CreateColorMask(tintedSource, new Scalar(0, 0, 150), new Scalar(100, 100, 255));
            Mat blueMask0 = CreateColorMask(tintedDestination0, new Scalar(0, 0, 150), new Scalar(100, 100, 255)); //new Scalar(150, 0, 0), new Scalar(255, 100, 100));

            Mat tintedDestination = ApplyTint(destination, Colors.Blue);
            Mat blueMask = ApplyTint(blueMask0, Colors.Blue);

            // Apply masks to original images
            Mat redHighlighted = ApplyMask(tintedSource, redMask);
            Mat blueHighlighted = ApplyMask(tintedDestination, blueMask);

            // Apply masks to original image
            // Combine the highlighted areas
            Mat combinedImage = new Mat();
            Cv2.AddWeighted(redHighlighted, 1.0, blueHighlighted, 1.0, 0.0, combinedImage);

            return combinedImage;
        }

        static Mat CreateColorMask(Mat tintedImage, Scalar lowerBound, Scalar upperBound)
        {
            Mat mask = new Mat();
            Cv2.InRange(tintedImage, lowerBound, upperBound, mask);
            return mask;
        }

        static Mat ApplyMask(Mat image, Mat mask)
        {
            Mat result = new Mat();
            Cv2.BitwiseAnd(image, image, result, mask);
            return result;
        }
               
        static Mat CombineMasks(Mat redTintedImage, Mat blueTintedImage, Mat redMask, Mat blueMask)
        {
            // Apply masks to the tinted images
            Mat redHighlighted = new Mat();
            Mat blueHighlighted = new Mat();
            Cv2.BitwiseAnd(redTintedImage, redTintedImage, redHighlighted, redMask);
            Cv2.BitwiseAnd(blueTintedImage, blueTintedImage, blueHighlighted, blueMask);

            // Combine the highlighted areas
            Mat combinedImage = new Mat();
            Cv2.AddWeighted(redHighlighted, 1.0, blueHighlighted, 1.0, 0.0, combinedImage);

            return combinedImage;
        }

        public Mat ApplyTint(Mat image, System.Windows.Media.Color tint)
        {
            // Convert the image to the same type as the input image if necessary
            if (image.Type() != MatType.CV_8UC3)
            {
                image.ConvertTo(image, MatType.CV_8UC3);
            }
            // Create a new Mat with the same size and type as the input image to store the tinted image
            Mat tintedImage = new Mat(image.Size(), image.Type());
            // Create a Scalar with the tint color
            Scalar tintScalar = new Scalar(tint.B, tint.G, tint.R); // Note the order BGR for OpenCV
                                                                    // Iterate through each pixel and apply the tint
            for (int i = 0; i < image.Rows; i++)
            {
                for (int j = 0; j < image.Cols; j++)
                {
                    Vec3b pixel = image.At<Vec3b>(i, j);
                    Vec3b tintedPixel = new Vec3b
                    {
                        Item0 = (byte)(pixel.Item0 * tintScalar.Val0 / 255),
                        Item1 = (byte)(pixel.Item1 * tintScalar.Val1 / 255),
                        Item2 = (byte)(pixel.Item2 * tintScalar.Val2 / 255)
                    };
                    tintedImage.Set(i, j, tintedPixel);
                }
            }
            return tintedImage;
        }
        #endregion
    }
}
