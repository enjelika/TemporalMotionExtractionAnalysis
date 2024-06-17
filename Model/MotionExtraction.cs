using System.IO;
using MOIE = Microsoft.Office.Interop.Excel;
using OpenCvSharp;
using OpenCvSharp.Quality;

namespace TemporalMotionExtractionAnalysis.Model
{
    internal class MotionExtraction
    {
        const string gif_root = "gifs";
        const string moca_folder = "MoCA";
        const string jpeg_images_folder = moca_folder + "\\JPEGImages";

        public MotionExtraction()
        {

        }

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
        public Mat ReduceAlpha(Mat image)
        {
            Mat modifiedImage;

            // Convert to grayscale
            Mat grayscale = Mat.Zeros(image.Width, image.Height);
            Cv2.CvtColor(image, grayscale, ColorConversionCodes.BGR2GRAY);

            // Convert the image to RGBA mode (if it's not already in RGBA mode)
            Cv2.CvtColor(grayscale, image, ColorConversionCodes.GRAY2RGBA);

            // Get pixel data
            int width = image.Width;
            int height = image.Height;

            // Create a new image to store the modified pixels
            modifiedImage = image.Clone(); // Clone to avoid modifying the input image directly

            // Iterate through each pixel and adjust opacity
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Reduce opacity based on the pixel intensity
                    if (modifiedImage.Channels() == 4)
                    {
                        Vec4b color = modifiedImage.At<Vec4b>(i, j);
                        double colorValue = (double)color.Item3;
                        color.Item3 = BitConverter.GetBytes((int)(colorValue * 0.5))[0];
                        modifiedImage.Set<Vec4b>(i, j, color);
                    }
                }
            }

            return modifiedImage;
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

        // Helper function: MAE evaluation of previous frame's motion vs current frame
        /// <summary>
        /// Calculates the Mean Absolute Error (MAE) between two image masks.
        /// </summary>
        /// <param name="prev_mask">The file path of the previous image mask.</param>
        /// <param name="curr_mask">The file path of the current image mask.</param>
        /// <returns>The Mean Absolute Error (MAE) between the two masks as a double value.</returns>
        public double CalculateMAE(string prev_mask, string curr_mask)
        {
            // Convert images to arrays for easier computation
            Mat prev_mask_mat = Cv2.ImRead(prev_mask);
            Mat curr_mask_mat = Cv2.ImRead(curr_mask);

            // Compute the absolute difference between the two masks
            Mat absolute_diff = Cv2.Abs(prev_mask_mat - curr_mask_mat);

            // Calculate the mean absolute error (MAE)
            double mae = (double)(Cv2.Mean(absolute_diff));
            return mae;
        }

        // Helper function: E_m evaluation of previous frame's motion vs current frame
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


        // Helper function: structural similarity index (SSIM) evaluation of previous frame's motion vs current frame
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

        // Source image masks destination image in composition image.
        // Source pixel is displayed if not black.
        /// <summary>
        /// Composes two images using the Source Over composition mode.
        /// </summary>
        /// <param name="source">The source image to be composited.</param>
        /// <param name="destination">The destination image where the source image is composited onto.</param>
        /// <returns>The composited image using the Source Over composition mode.</returns>
        public Mat SourceOver (Mat source, Mat destination)
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

        // Destination image masks source destination image in composition image.
        // Destination pixel is displayed if not black.
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

        // The intersection of source and destination images 
        // is displayed as the source color.
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

        // The intersection of source and destination images 
        // is displayed as the destination color.
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

        // The intersection of source and destination images 
        // is displayed as the source color.
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

        // The area of the destination image not intersecting
        // the source image is displayed as the destination color.
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

        // The intersection of source and destination images 
        // is displayed as the source color and the remaining destination
        // area is displayed also.
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

        // The intersection of source and destination images 
        // is displayed as the destination color and the remaining source
        // area is displayed also.
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

        // The areas of source and destination images that
        // are mutually exclusive are each displayed.
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
        }

        //""" Helper function: Make Motion Extracted GIF from images in a folder location """ 
        //def motion_extraction(frame_folder, class_name, output_folder= "./output_masks"):
        //    # Record the start time
        //    start_time = time.time()

        //# Get all of the current gif frames
        //    frames = glob.glob(f"{frame_folder}/*.JPG")
        //    num_frames = len(frames)
        //    print(f"Number of frames: {num_frames}")

        //    alpha = 50
        //    index = prev_frame

        //    # Define a static blur radius for all frames
        //    blur_radius = 5

        //    # Create output folders
        //    mask_imgs_folder = os.path.join(output_folder, f"{class_name}")
        //    os.makedirs(mask_imgs_folder, exist_ok=True)
        //    output_subfolder = os.path.join(output_folder, os.path.basename(frame_folder))
        //    os.makedirs(output_subfolder, exist_ok=True)

        //    # Pre-process frames with tqdm for a progress bar
        //    for frame_path in tqdm(frames, desc= f"Pre-processing frames for {class_name}") :
        //        image = Image.open(frame_path)

        //# Check if the mask image already exists
        //        mask_save_path = os.path.join(output_subfolder, f"mask_{index}.png")
        //        if os.path.exists(mask_save_path):
        //            # print(f"Skipping frame {index} as the mask already exists.")
        //            index += 1
        //            continue

        //        # Step 1 - Invert colors      
        //        inverted = ImageOps.invert(image)
        //        blended = inverted.convert("RGBA")
        //        # TODO: Save first frame's output
        //        # Save first frame's output
        //        if index == 60:
        //            blended.save(os.path.join(output_subfolder, "step1_sixtith_frame.png"))

        //        # Step 2 - Reduce opacity (aka alpha)
        //        blended.putalpha(alpha)
        //        semifinal = ReduceAlpha(blended)
        //        semifinal.putalpha(alpha)
        //        # TODO: Save first frame's output
        //        if index == 60:
        //            semifinal.save(os.path.join(output_subfolder, "step2_sixtith_frame.png"))        

        //        # Step 3 - Add blur
        //        final = semifinal.filter(ImageFilter.GaussianBlur(radius=blur_radius))
        //        # TODO: Save first frame's output
        //        if index == 60:
        //            final.save(os.path.join(output_subfolder, "step3_sixtith_frame.png"))

        //        # show_image_with_index(final, index) # For troubleshooting

        //        # Save the mask image to disk
        //        mask_save_path = os.path.join(mask_imgs_folder, f"mask_{index}.png")
        //        final.save(mask_save_path)

        //        index += 1

        //    # Metrics calculation outside the loop
        //    maes = []
        //        ems = []
        //        ssims = []
        //        blended_imgs = []

        //# Open one of the frames to get its dimensions
        //        sample_frame = Image.open(os.path.join(mask_imgs_folder, "mask_0.png"))
        //    width, height = sample_frame.size

        //# Initialize an empty blended image folder
        //    blended_imgs_folder = os.path.join("./blended_imgs", f"{class_name}")
        //    os.makedirs(blended_imgs_folder, exist_ok=True)

        //    # Initialize an empty composite image
        //    # composite_image = Image.new('RGBA', (width, height), (0, 0, 0, 0))
        //    # composite_imgs_folder = os.path.join("./composite_masks", f"{class_name}")
        //    # os.makedirs(composite_imgs_folder, exist_ok=True)

        //    # Initialize the 'final' variable
        //    final = Image.new ('RGBA', (width, height), (0, 0, 0, 0))

        //    # Specify the total number of iterations for tqdm
        //    total_iterations = num_frames - 1

        //    # Process frames with tqdm for a progress bar
        //    for index in tqdm(range(1, num_frames), total=total_iterations, desc=f"Processing frames for {class_name}"):
        //        prev_mask_path = os.path.join(mask_imgs_folder, f"mask_{index - 1}.png")
        //        curr_mask_path = os.path.join(mask_imgs_folder, f"mask_{index}.png")

        //        # Load the mask images only when needed
        //        prev_mask_img = Image.open(prev_mask_path)
        //        curr_mask_img = Image.open(curr_mask_path)

        //        # Convert the composite image to grayscale for metric calculations
        //        prev_mask_array = np.array(prev_mask_img.convert('L'))

        //        # Convert the current mask image to grayscale for metric calculations
        //        curr_mask_array = np.array(curr_mask_img.convert('L'))

        //        # Calculate MAE
        //        mae = np.mean(np.abs(prev_mask_array - curr_mask_array))
        //        maes.append(mae)

        //# Calculate E-measure
        //        em = CalculateEmeasurePixelwise(prev_mask_array, curr_mask_array)
        //        ems.append(em)

        //# Calculate SSIM
        //        ssim = CalculateSSIM(prev_mask_array, curr_mask_array)
        //        ssims.append(ssim)

        //# TODO: Overlay metrics on top of frame as a PCA scatter plot
        //# Instead of a vanilla scatter plot - no axis, use glyphs for the values
        //# triangle up - value increased from last frame
        //# triangle down - value decreased from last frame
        //# circle - value remained the same from last frame
        //# use of colors to distinguish the different metrics:
        //# purple - MAE
        //# blue - E-measure
        //# green - SSIM
        //# -- labels for the exact value?

        //# MAE

        //# E-measure

        //# SSIM


        //# Append the current mask image directly to blended_imgs
        //        blended_imgs.append(curr_mask_img)

        //        # # Blend the current frame with the composite image
        //# blended_frame = overlay_images(curr_mask_img, prev_mask_img)
        //# blended_imgs.append(blended_frame)

        //# # Update the composite image for the next iteration
        //# composite_image = Image.blend(composite_image, curr_mask_img, alpha=0.5)

        //# Save the final blended image to disk
        //        blended_save_path = os.path.join(blended_imgs_folder, f"blended_{index}.png")
        //        curr_mask_img.save(blended_save_path)

        //    # Create Motion Results GIF
        //    if blended_imgs:
        //        output_gif_path = f"./results/{class_name}_motion_results.gif"
        //        blended_imgs[0].save(
        //            output_gif_path,
        //            format= "GIF",
        //            append_images= blended_imgs[1:],
        //            save_all= True,
        //            duration= 100,
        //            loop= 0
        //        )


        //        with imageio.get_writer(output_gif_path, mode= 'I', duration= 0.1) as writer:
        //            for img in blended_imgs:
        //                writer.append_data(np.array(img))

        //    # Save metrics to a dictionary
        //    metrics_dict = {
        //        'maes': maes,
        //        'ems': ems,
        //        'ssims': ssims
        //    }

        //# Specify the path for the JSON file
        //    json_file_path = f"./results/{class_name}_metrics_results.json"

        //    # Save the metrics dictionary to the JSON file
        //    with open(json_file_path, 'w') as json_file:
        //        json.dump(metrics_dict, json_file)

        //# Calculate averages
        //    average_MAE = np.nanmean(maes)
        //    average_Em = np.nanmean(ems)
        //    average_SSIM = np.nanmean(ssims)

        //    # Check for NaN and Inf and replace with 0
        //    average_MAE = 0 if np.isnan(average_MAE) or np.isinf(average_MAE) else average_MAE
        //    average_Em = 0 if np.isnan(average_Em) or np.isinf(average_Em) else average_Em
        //    average_SSIM = 0 if np.isnan(average_SSIM) or np.isinf(average_SSIM) else average_SSIM

        //    print(f"Avg. MAE: {average_MAE}, Avg. E-measure: {average_Em}, Avg. SSIM: {average_SSIM}")

        //    end_time = time.time()  # Record the end time
        //    elapsed_time = end_time - start_time

        //    print(f"Motion extraction for {class_name} completed in {elapsed_time:.2f} seconds.")

        //    return average_MAE, average_Em, average_SSIM

        //"""
        //===================================================================================================
        //    Helper function
        //        - Visualization function
        //        -- reads in a json file of MAE, E_m, and SSIM
        //        -- accesses sequential frames in 'output_masks' folder
        //===================================================================================================
        //"""
        //def results_visualization(json_file_path, output_masks_path, folder_name):
        //    # Load metrics from the JSON file
        //    with open(json_file_path, 'r') as json_file:
        //        loaded_metrics = json.load(json_file)

        //# Access the lists of metrics
        //    loaded_maes = loaded_metrics['maes']
        //    loaded_ems = loaded_metrics['ems']
        //    loaded_ssims = loaded_metrics['ssims']

        //    # Get the list of mask images from the 'output_masks' folder
        //    mask_files = sorted(glob.glob(os.path.join(output_masks_path, '*.png')))

        //    # Check if the number of loaded metrics matches the number of mask images
        //    if len(loaded_maes) != len(mask_files) - 1 or len(loaded_ems) != len(mask_files) - 1 or len(loaded_ssims) != len(mask_files) - 1:
        //        print("Mismatch between the number of metrics and mask images.")
        //        return

        //    # Create a DataFrame for Parallel Coordinates Plot
        //    df = pd.DataFrame({
        //        'Frame Index': range(1, len(mask_files)),
        //        'MAE': loaded_maes, # Weak to no linear relationship
        //        'E-measure': loaded_ems, # moderate to strong negative linear relationship
        //        'SSIM': loaded_ssims #, # moderate to strong negative linear relationship
        //        #'Thumbnail': mask_files[1:]  # Add a column for thumbnail file paths
        //    })

        //    # We need to transform the data from raw data to a normalized value
        //    data = df[['Frame Index', 'MAE', 'E-measure', 'SSIM']]

        //    #==========================================================================

        //# Create a scatter plot
        //# plt.scatter(data["Frame Index"], data["SSIM"])
        //# plt.title('Scatter Plot of SSIM vs Frame Index')
        //# plt.xlabel('Frame Index')
        //# plt.ylabel('SSIM')
        //# plt.show()

        //# # Compute the correlation coefficient
        //# correlation_coefficient = np.corrcoef(data["Frame Index"], data["SSIM"])[0, 1]
        //# print(f'SSIM Correlation Coefficient: {correlation_coefficient}')

        //#==========================================================================

        //# Set up the figure and axes
        //    fig, (ax_linear, ax_nonlinear) = plt.subplots(2, 1, sharex = True, figsize = (10, 6))

        //    # Plot linear metrics (E-measure and SSIM)
        //    sns.lineplot(x='Frame Index', y = 'E-measure', data = df, label = 'E-measure', ax = ax_linear)
        //    sns.lineplot(x='Frame Index', y = 'SSIM', data = df, label = 'SSIM', ax = ax_linear)
        //    ax_linear.set_title('Linear Metrics')

        //    # Plot non-linear metric (MAE)
        //    sns.barplot(x='Frame Index', y = 'MAE', data = df, ax = ax_nonlinear, color = 'skyblue')
        //    ax_nonlinear.set_title('Non-linear Metric')

        //    # Adjust layout
        //    plt.tight_layout()
        //    plt.show()

        //    # Normalize values using Min-Max scaling
        //    # scaler = MinMaxScaler()
        //    # normalized_data = data.set_index('Frame Index')
        //    # normalized_data = pd.DataFrame(scaler.fit_transform(normalized_data), columns=normalized_data.columns, index=normalized_data.index)

        //    # # Make the plot
        //    # plt.stackplot(normalized_data.index, normalized_data["MAE"], normalized_data["E-measure"], normalized_data["SSIM"], labels=['MAE', 'E-measure', 'SSIM'])
        //    # plt.legend(loc='upper left')
        //    # plt.margins(0, 0)
        //    # plt.title('Normalized Stacked Area Chart')
        //    # plt.xlabel('Frame Index')
        //    # plt.ylabel('Normalized Value')

        //    # plt.show()

        //    # Resetting the index of normalized_data
        //    # normalized_data_reset = normalized_data.reset_index()

        //    # # Set up the matplotlib figure
        //    # plt.figure(figsize=(10, 6))

        //    # # Distplot
        //    # group_labels = ['MAE', 'E-measure', 'SSIM']

        //    # # Create distplot with Seaborn
        //    # for label in group_labels:
        //    #     sns.distplot(data[label], bins=20, kde=True, label=label)

        //    # # Show legend
        //    # plt.legend()

        //    # # Add title and labels
        //    # plt.title('Distribution of Metrics')
        //    # plt.xlabel('Values')
        //    # plt.ylabel('Density')

        //    # # Show the plot
        //    # plt.show()

        //        # Create a subplot with four rows
        //    # fig = make_subplots(rows=3, cols=1, shared_xaxes=True, subplot_titles=['Thumbnails', 'MAEs', 'E-measures', 'SSIMs'])

        //    # # Add Thumbnails to the first row
        //    # thumbnails = [Image.open(thumbnail_path).resize((50, 50)) for thumbnail_path in df['Thumbnail']]
        //    # for i, thumbnail in enumerate(thumbnails, start=1):
        //    #     # Convert the NumPy array to a PIL.Image.Image object
        //    #     pil_image = Image.fromarray(np.array(thumbnail))

        //    #     # Create a BytesIO object to store the image in memory
        //    #     image_bytes = io.BytesIO()

        //    #     # Save the PIL.Image.Image to the BytesIO object in PNG format
        //    #     pil_image.save(image_bytes, format='PNG')

        //    #     # Convert the BytesIO object to a base64-encoded image URI
        //    #     image_uri = f"data:image/png;base64,{base64.b64encode(image_bytes.getvalue()).decode()}"

        //    #     # Add image to the layout
        //    #     fig.add_trace(go.Image(source=image_uri), row=1, col=1)

        //    # Add Frame Index as a number line to the first row
        //    # fig.add_trace(go.Scatter(x=df['Frame Index'], y=[0] * len(df['Frame Index']), mode='markers', marker=dict(size=10), showlegend=False), row=1, col=1)

        //    # # Add MAEs as heatmap to the second row
        //    # fig.add_trace(go.Histogram2d(x=df['Frame Index'], y=df['MAE'], colorscale='PRGn', showlegend=False), row=1, col=1)

        //    # # Add E-measures as heatmap to the third row
        //    # fig.add_trace(go.Histogram2d(x=df['Frame Index'], y=df['E-measure'], colorscale='PRGn', showlegend=False), row=2, col=1)

        //    # # Add SSIMs as heatmap to the fourth row
        //    # fig.add_trace(go.Histogram2d(x=df['Frame Index'], y=df['SSIM'], colorscale='PRGn', showlegend=False), row=3, col=1)

        //    # # Update layout
        //    # fig.update_layout(height=800, width=800)

        //    # Save the interactive plot as an HTML file
        //    # plot(fig, filename=f"./visualizations/{folder_name}_motion_extraction_interactive_visualization.html")

        //    # # Create a subplot for Thumbnails
        //    # fig_images = make_subplots(
        //    #     rows=len(thumbnails), cols=1,
        //    #     subplot_titles=[""] * len(thumbnails)
        //    # )

        //    # # Add Thumbnails
        //    # for i, thumbnail in enumerate(tqdm(thumbnails, desc="Adding Thumbnails", unit="image", position=0, leave=True), start=1):
        //    #     # Convert the NumPy array to a PIL.Image.Image object
        //    #     pil_image = Image.fromarray(np.array(thumbnail))

        //    #     # Create a BytesIO object to store the image in memory
        //    #     image_bytes = io.BytesIO()

        //    #     # Save the PIL.Image.Image to the BytesIO object in PNG format
        //    #     pil_image.save(image_bytes, format='PNG')

        //    #     # Convert the BytesIO object to a base64-encoded image URI
        //    #     image_uri = f"data:image/png;base64,{base64.b64encode(image_bytes.getvalue()).decode()}"

        //    #     # Add Image to subplot
        //    #     # fig_images.add_trace(go.Image(source=image_uri),
        //    #     #                      row=i, col=1)
        //    #     # Add scatter trace with Image mode
        //    #     fig_images.add_trace(go.Scatter(
        //    #         x=[0],
        //    #         y=[1 - i / len(thumbnails)],
        //    #         mode='markers',
        //    #         marker=dict(size=1, opacity=0),
        //    #         hoverinfo='text',
        //    #         text=[image_uri],
        //    #     ), row=i, col=1)

        //    # # Save Thumbnails as a separate HTML file
        //    # thumbnails_html_path = f"./visualizations/{folder_name}_thumbnails_visualization.html"
        //    # plot(fig_images, filename=thumbnails_html_path, auto_open=False)

        //    # # Create a parallel coordinates plot using plotly.graph_objects
        //    # parallel_plot = go.FigureWidget(
        //    #     go.Parcoords(
        //    #         line=dict(color=df['Frame Index'], colorscale='Viridis'),
        //    #         dimensions=[
        //    #             dict(label='MAE', values=df['MAE']),
        //    #             dict(label='E-measure', values=df['E-measure']),
        //    #             dict(label='SSIM', values=df['SSIM'])
        //    #         ]
        //    #     )
        //    # )

        //    # # Save Parallel Coordinates Plot as a separate HTML file
        //    # parallel_plot_html_path = f"./visualizations/{folder_name}_parallel_coordinates_plot.html"
        //    # plot(parallel_plot, filename=parallel_plot_html_path, auto_open=False)

        //    # # Combine Thumbnails and Parallel Coordinates Plot in a single HTML file using custom HTML and CSS
        //    # combined_html_content = f"""
        //    # <html>
        //    #     <head>
        //    #         <style>
        //    #             .container {{
        //    #                 display: flex;
        //    #                 flex-direction: column;
        //    #             }}
        //    #             .thumbnails {{
        //    #                 width: 100%;
        //    #                 flex: 1;
        //    #             }}
        //    #             .parallel-plot {{
        //    #                 width: 100%;
        //    #                 flex: 1;
        //    #             }}
        //    #         </style>
        //    #     </head>
        //    #     <body>
        //    #        <div class="container">
        //    #             <div class="thumbnails">
        //    #                 {fig_images.to_html(full_html=False)}
        //    #             </div>
        //    #             <div class="parallel-plot">
        //    #                 {parallel_plot.to_html(full_html=False)}
        //    #             </div>
        //    #         </div>
        //    #     </body>
        //    # </html>
        //    # """

        //    # # Save the combined HTML file
        //    # combined_html_path = f"./visualizations/{folder_name}_combined_visualization.html"
        //    # with open(combined_html_path, 'w', encoding='utf-8') as combined_html_file:
        //    #     combined_html_file.write(combined_html_content)

        //    # # Create figure with parallel coordinates plot
        //    # fig = px.parallel_coordinates(
        //    #     df,
        //    #     color=df['Frame Index'],  # Use Frame Index for color
        //    #     labels={'MAE': 'MAE', 'E-measure': 'E-measure', 'SSIM': 'SSIM', 'Frame Index': 'Frame Index'},
        //    #     color_continuous_scale=px.colors.sequential.Viridis,
        //    #     title='Motion Extraction Metrics - Parallel Coordinates Plot'
        //    # )

        //    # # Add Thumbnails
        //    # thumbnails = [Image.open(thumbnail_path) for thumbnail_path in df['Thumbnail']]

        //    # # Calculate the height needed for thumbnails
        //    # thumbnail_height = 300 * len(thumbnails)

        //    # # Add images as annotations to the figure
        //    # for i, thumbnail in enumerate(tqdm(thumbnails, desc="Adding Thumbnails", unit="image", position=0, leave=True), start=1):
        //    #     # Convert the NumPy array to a PIL.Image.Image object
        //    #     pil_image = Image.fromarray(np.array(thumbnail))

        //    #     # Create a BytesIO object to store the image in memory
        //    #     image_bytes = io.BytesIO()

        //    #     # Save the PIL.Image.Image to the BytesIO object in PNG format
        //    #     pil_image.save(image_bytes, format='PNG')

        //    #     # Convert the BytesIO object to a base64-encoded image URI
        //    #     image_uri = f"data:image/png;base64,{base64.b64encode(image_bytes.getvalue()).decode()}"

        //    #     fig.add_layout_image(
        //    #         source=image_uri,
        //    #         x=0,
        //    #         y=1 - i / len(thumbnails),
        //    #         xanchor='left',
        //    #         yanchor='bottom',
        //    #         sizex=1,
        //    #         sizey=1 / len(thumbnails),
        //    #         xref='paper',
        //    #         yref='paper'
        //    #     )

        //    # # Update layout for the combined figure
        //    # fig.update_layout(
        //    #     height=thumbnail_height,
        //    #     showlegend=False
        //    # )

        //    # Save the interactive plot as an HTML file
        //    # plot(fig, filename=f"./visualizations/{folder_name}_motion_extraction_interactive_visualization.html")


        //def extract_frames(input_gif, output_folder):
        //    # Open the GIF file
        //    gif = Image.open(input_gif)

        //    # Iterate through each frame in the GIF
        //    for frame_index in range(gif.n_frames):
        //        gif.seek(frame_index)  # Go to the specific frame
        //        frame = gif.copy()  # Copy the frame

        //        # Convert the frame to RGB mode if it's in palette mode or RGBA mode
        //        if frame.mode == 'P':
        //            frame = frame.convert('RGB')
        //        elif frame.mode == 'RGBA':
        //            frame = frame.convert('RGB')  # Convert RGBA to RGB

        //        # Save the frame as an individual image (JPEG)
        //        output_path = os.path.join(output_folder, f"frame_{frame_index}.jpg")
        //        frame.save(output_path, format = "JPEG")


        //"""
        //===================================================================================================
        //    Helper function
        //        - Segments the detected motion in yellow and green
        //===================================================================================================
        //"""
        //def overlay_images(original_image, mask_image):
        //    # Resize the mask image to match the original image's size
        //    mask_image = mask_image.resize(original_image.size, Image.LANCZOS).convert("RGBA")


        //    # Print dimensions for debugging
        //# print("Original Image Dimensions:", original_image.size)
        //# print("Mask Image Dimensions:", mask_image.size)
        //# Convert both images to RGBA mode
        //    original_image = original_image.convert("RGBA")
        //    mask_image = mask_image.convert("RGBA")

        //    # Blend the images
        //    try:
        //        blended_image = Image.blend(original_image, mask_image, alpha = 0.2)
        //    except ValueError as e:
        //        print(f"Error: {e}")
        //        return None

        //    # Save the resulting blended image (optional)
        //    blended_image.save("blended_image.png")

        //    return blended_image


        //def is_folder_empty(folder_path):
        //    return len(os.listdir(folder_path)) == 0


        //def folder_has_multiple_files(folder_path):
        //    if not os.path.exists(folder_path) or not os.path.isdir(folder_path):
        //        return False  # If the folder doesn't exist or is not a directory

        //    files = [file for file in os.listdir(folder_path) if os.path.isfile(os.path.join(folder_path, file))]
        //    return len(files) > 1


        //def count_files_in_directory(directory):
        //    return len([file for file in os.listdir(directory) if os.path.isfile(os.path.join(directory, file))])


        //def combine_gif_frames_for_pub_figure(gif_path):
        //    # Open the GIF and select the frames you want (0-indexed)
        //    gif = Image.open(gif_path)

        //    # Get the width and height of the GIF frames
        //    width, height = gif.size

        //    # Select the frames you want (assuming they are 4 sequential frames)
        //    frame1 = gif.copy().convert("RGBA").resize((width, height))
        //    gif.seek(1)
        //    frame2 = gif.copy().convert("RGBA").resize((width, height))
        //    gif.seek(2)
        //    frame3 = gif.copy().convert("RGBA").resize((width, height))
        //    gif.seek(3)
        //    frame4 = gif.copy().convert("RGBA").resize((width, height))

        //    # Create a new blank image to paste the frames
        //    combined_image = Image.new('RGBA', (width * 2, height * 2))

        //    # Paste frames onto the new image
        //    combined_image.paste(frame1, (0, 0))
        //    combined_image.paste(frame2, (width, 0))
        //    combined_image.paste(frame3, (0, height))
        //    combined_image.paste(frame4, (width, height))

        //    # Save the combined image
        //    combined_image.save("combined_image.png")


        //"""
        //===================================================================================================
        //    Main
        //===================================================================================================
        //"""
        public MotionExtraction(string selectedPath)
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
    }
}
