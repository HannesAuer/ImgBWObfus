using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Img_BW_Obfus
{
    /// <summary>
    /// Class for:
    /// - loading png's from a folder
    /// - Encoding:
    ///     - a text into an image
    ///     - a file into an image
    ///     - and encrypt a text into an image
    ///     - and encrypt a file into an image
    /// - Decoding:
    ///     - image to a text
    ///     - image to a file
    ///     - encrypted image to a text
    ///     - encrypted image to a file
    ///     
    /// ############ By H4nn3s ############
    /// 
    /// Date: 22.03.2023
    /// </summary>
    internal class ImgBWObfus
    {
        #region Load Png
        /// <summary>
        /// Loads a PNG image from the specified file path.
        /// E.g. LoadPngFromFolder("C:\\Users\\User\\Desktop\\Image.png");
        /// </summary>
        /// <param name="imagePath">The file path of the PNG image.</param>
        /// <returns>A Bitmap object containing the loaded image.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file path does not exist.</exception>
        public static Bitmap LoadPngFromFolder(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("File not found", imagePath);

            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var bitmap = new Bitmap(fileStream);
                return bitmap;
            }
        }

        /// <summary>
        /// Loads all PNG images matching the specified base file name from a specified folder path.
        /// Note: "\\" at the end of the folderPath is important!
        /// Note: Files must have the following structure: {Filename}_{iteration number}.png
        /// E.g. LoadPngsFromFolder("C:\\Users\\User\\Desktop\\", "BaseFileName");
        /// </summary>
        /// <param name="folderPath">The folder path containing the PNG images.</param>
        /// <param name="baseFileName">The base file name of the PNG images to load.</param>
        /// <returns>A List of Bitmap objects containing the loaded images.</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified folder path does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown if no PNG files matching the specified base file name are found in the specified folder path.</exception>
        public static List<Bitmap> LoadPngsFromFolder(string folderPath, string baseFileName)
        {
            List<Bitmap> bitmapList = new List<Bitmap>();
            IEnumerable<string> pngFiles = new List<string>();

            // Searches for valid files
            if (Directory.Exists(folderPath))
                pngFiles = Directory.GetFiles(folderPath, $"{baseFileName}_*.png")
                    .OrderBy(filePath => GetIterationNumber(filePath));
            else throw new DirectoryNotFoundException("The specified directory was not found.");

            if (!pngFiles.Any())
                throw new FileNotFoundException("No files were found in the specified folder.", Path.Combine(folderPath, baseFileName + "_*.png"));

            // Convert them into a bitmap
            foreach (var pngFilePath in pngFiles)
            {
                using (var fileStream = new FileStream(pngFilePath, FileMode.Open, FileAccess.Read))
                {
                    var bitmap = new Bitmap(fileStream);
                    bitmapList.Add(bitmap);
                }
            }

            return bitmapList;
        }

        /// <summary>
        /// Extracts the iteration number from the file name given in the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file whose iteration number is to be extracted.</param>
        /// <returns>The iteration number of the file.</returns>
        /// <exception cref="ArgumentException">Thrown when the iteration number cannot be parsed as an integer.</exception>
        private static int GetIterationNumber(string filePath)
        {
            // Check for iteration number
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var lastUnderscoreIndex = fileName.LastIndexOf('_');
            var iterationNumberStr = fileName.Substring(lastUnderscoreIndex + 1);
            var isNumeric = int.TryParse(iterationNumberStr, out int iterationNumber);

            if (isNumeric)
                return iterationNumber;
            else
                throw new ArgumentException("Invalid filename format: iteration number is not a valid integer.");
        }
        #endregion Load Png

        #region Encoding
        #region Text
        /// <summary>
        /// Converts the specified text into a black/white bitmap image.
        /// Note: outputImagePath without extension! Default extension is png.
        /// E.g. TextToImage("Text to be converted.", "C:\\Users\\User\\Desktop\\OutputImage");
        /// </summary>
        /// <param name="text">The text to be converted into an image.</param>
        /// <param name="outputImagePath">The path where the output image file is to be saved.</param>
        /// <returns>A black/white bitmap image of the specified text.</returns>
        public static Bitmap TextToImage(string text, string outputImagePath = null)
        {
            return BinaryStrToImage(AddCountToBinaryStr(TextToBinaryStr(text)), outputImagePath);
        }

        /// <summary>
        /// Converts the specified text into a bitmap image, with added salt values to the binary representation of the text.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// Note: Salt numbers only make sense between 0 and 7 included. They should not occur twice, but can be grouped together in any length.
        /// Note: Salt number must be remembered otherwise the file cannot be restored.
        /// E.g. int[] salt = { 1, 3 }; or int[] salt = { 1, 3, 5, 7 }; or int[] salt = {0, 1, 2, 3, 4, 5, 6, 7}; etc.
        /// E.g. TextToImage("Text to be converted.", new int[] { 2, 4, 5 }, "C:\\Users\\User\\Desktop\\OutputImage");
        /// </summary>
        /// <param name="text">The text to be converted into an image.</param>
        /// <param name="outputImagePath">The path where the output image file is to be saved.</param>
        /// <returns>A black/white encrypted bitmap image of the specified text.</returns>
        public static Bitmap TextToImage(string text, int[] salt, string outputImagePath = null)
        {
            return BinaryStrToImage(BinaryStrToSalt(AddCountToBinaryStr(TextToBinaryStr(text)), salt), outputImagePath);
        }

        /// <summary>
        /// Converts the specified text into a list of bitmap images, each containing a portion of the text, with a default image size of 1920x1080.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// E.g. TextToImage("Text to be converted into multiple images.", 800, 600, "C:\Users\User\Desktop\OutputImage");
        /// </summary>
        /// <param name="text">The text to be converted into a list of images.</param>
        /// <param name="width">The width of each image in pixels (default is 1920).</param>
        /// <param name="height">The height of each image in pixels (default is 1080).</param>
        /// <param name="outputImagePath">The path where the output image files are to be saved.</param>
        /// <returns>A list of black/white bitmap images of the specified text.</returns>
        public static List<Bitmap> TextToImage(string text, int width = 1920, int height = 1080, string outputImagePath = null)
        {
            return BinaryStrToImageList(AddCountToBinaryStr(TextToBinaryStr(text)), width, height, outputImagePath);
        }

        /// <summary>
        /// Converts the specified text into a list of bitmap images, with added salt values to the binary representation of the text.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// Note: Salt numbers only make sense between 0 and 7 included. They should not occur twice, but can be grouped together in any length.
        /// Note: Salt number must be remembered otherwise the file cannot be restored.
        /// E.g. int[] salt = { 1, 3 }; or int[] salt = { 1, 3, 5, 7 }; or int[] salt = {0, 1, 2, 3, 4, 5, 6, 7}; etc.
        /// E.g. TextToImage("Text to be converted.", new int[] { 2, 4, 5 }, 1920, 1080, "C:\Users\User\Desktop\OutputImage");
        /// </summary>
        /// <param name="text">The text to be converted into a list of bitmap images.</param>
        /// <param name="salt">An array of salt values to be added to the binary representation of the text.</param>
        /// <param name="width">The width of each image in pixels (default is 1920).</param>
        /// <param name="height">The height of each image in pixels (default is 1080).</param>
        /// <param name="outputImagePath">The path where the output image files are to be saved.</param>
        /// <returns>A list of black/white encrypted bitmap images of the specified text.</returns>
        public static List<Bitmap> TextToImage(string text, int[] salt, int width = 1920, int height = 1080, string outputImagePath = null)
        {
            return BinaryStrToImageList(BinaryStrToSalt(AddCountToBinaryStr(TextToBinaryStr(text)), salt), width, height, outputImagePath);
        }
        #endregion Text

        #region File
        /// <summary>
        /// Reads a file from the specified file path and converts it into a black/white bitmap image.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// E.g. FileToImage("C:\\Users\\User\\Desktop\\InputFile.exe", "C:\\Users\\User\\Desktop\\OutputImage");
        /// </summary>
        /// <param name="filePath">The path of the file to be converted into an image.</param>
        /// <param name="outputImagePath">The path where the output image file is to be saved.</param>
        /// <returns>A black/white bitmap image of the file contents.</returns>
        public static Bitmap FileToImage(string filePath, string outputImagePath = null)
        {
            // Get the file bytes and convert them
            byte[] bytes = File.ReadAllBytes(filePath);
            string binaryStr = string.Join("", bytes.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            binaryStr = AddCountToBinaryStr(binaryStr);

            Bitmap bitmap = BinaryStrToImage(binaryStr, outputImagePath);

            return bitmap;
        }

        /// <summary>
        /// Reads a file from the specified file path and converts it into a black/white bitmap image with added salt values.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// Note: Salt numbers only make sense between 0 and 7 included. They should not occur twice, but can be grouped together in any length.
        /// Note: Salt number must be remembered otherwise the file cannot be restored.
        /// E.g. int[] salt = { 1, 3 }; or int[] salt = { 1, 3, 5, 7 }; or int[] salt = {0, 1, 2, 3, 4, 5, 6, 7}; etc.
        /// E.g. FileToImage("C:\Users\User\Desktop\InputFile.exe", new int[] { 2, 4, 5 }, "C:\Users\User\Desktop\OutputImage");
        /// </summary>
        /// <param name="filePath">The path of the file to be converted into an image.</param>
        /// <param name="salt">The salt values to be added to the binary representation of the file contents.</param>
        /// <param name="outputImagePath">The path where the output image file is to be saved.</param>
        /// <returns>A black/white encrypted bitmap image of the file contents with added salt values.</returns>
        public static Bitmap FileToImage(string filePath, int[] salt, string outputImagePath = null)
        {
            // Get the file bytes and convert them
            byte[] bytes = File.ReadAllBytes(filePath);
            string binaryStr = string.Join("", bytes.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            binaryStr = BinaryStrToSalt(AddCountToBinaryStr(binaryStr), salt);

            Bitmap bitmap = BinaryStrToImage(binaryStr, outputImagePath);

            return bitmap;
        }

        /// <summary>
        /// Reads a file from the specified file path and converts it into a list of black/white bitmap images.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// E.g. FileToImage("C:\\Users\\User\\Desktop\\InputFile.exe", 800, 600, "C:\\Users\\User\\Desktop\\OutputImage");
        /// </summary>
        /// <param name="filePath">The path of the file to be converted into images.</param>
        /// <param name="width">The width of the output images.</param>
        /// <param name="height">The height of the output images.</param>
        /// <param name="outputImagePath">The path where the output image files are to be saved.</param>
        /// <returns>A list of black/white bitmap images of the file contents.</returns>
        public static List<Bitmap> FileToImage(string filePath, int width = 1920, int height = 1080, string outputImagePath = null)
        {
            return FileToImage(filePath, null, width, height, outputImagePath);
        }

        /// <summary>
        /// Reads a file from the specified file path and converts it into a black/white bitmap image with added salt values.
        /// Note: OutputImagePath without extension! Default extension is png.
        /// Note: Salt numbers only make sense between 0 and 7 included. They should not occur twice, but can be grouped together in any length.
        /// Note: Salt number must be remembered otherwise the file cannot be restored.
        /// E.g. int[] salt = { 1, 3 }; or int[] salt = { 1, 3, 5, 7 }; or int[] salt = {0, 1, 2, 3, 4, 5, 6, 7}; etc.
        /// E.g. FileToImage("C:\Users\User\Desktop\InputFile.exe", new int[] { 2, 4, 5 }, 800, 600, "C:\Users\User\Desktop\OutputImage");
        /// </summary>
        /// <param name="filePath">The path of the file to be converted into an image.</param>
        /// <param name="salt">The salt values to be added to the binary representation of the file contents.</param>
        /// <param name="width">The width of the output bitmap image. Default is 1920.</param>
        /// <param name="height">The height of the output bitmap image. Default is 1080.</param>
        /// <param name="outputImagePath">The path where the output image file is to be saved. The default extension is png.</param>
        /// <returns>A list of black/white encrypted bitmap images of the file contents with added salt values.</returns>
        public static List<Bitmap> FileToImage(string filePath, int[] salt, int width = 1920, int height = 1080, string outputImagePath = null)
        {
            List<Bitmap> bitmapList = new List<Bitmap>();
            // Open the file stream
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Create a buffer to read the file
                byte[] buffer = new byte[width * height / 8];
                int bytesRead;

                // Prepare the prefix
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSizeInBytes = fileInfo.Length;
                string prefix = (fileSizeInBytes * 8).ToString() + '!';

                // Read the file in chunks and process each chunk as a bitmap
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert the chunk to a binary string representation
                    string binaryStr = string.Join("", buffer.Take(bytesRead).Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));

                    if (bitmapList.Count == 0)
                    {
                        binaryStr = TextToBinaryStr(prefix) + binaryStr;
                        fileStream.Seek(buffer.Length - prefix.Length, SeekOrigin.Begin);
                    }

                    if (salt != null)
                        binaryStr = BinaryStrToSalt(binaryStr, salt);

                    // If needed safe Image
                    Bitmap bitmap;
                    if (outputImagePath != null)
                        bitmap = BinaryStrToImage(binaryStr, width, height, $"{outputImagePath}_{bitmapList.Count}");
                    else
                        bitmap = BinaryStrToImage(binaryStr, width, height);

                    bitmapList.Add(bitmap);
                }
            }
            return bitmapList;
        }
        #endregion File

        #region Helper
        /// <summary>
        /// Converts a given string into its binary string representation.
        /// </summary>
        /// <param name="text">The input text to convert.</param>
        /// <returns>A binary string representation of the input text.</returns>
        private static string TextToBinaryStr(string text)
        {
            byte[] byteArr = Encoding.ASCII.GetBytes(text);
            string byteStr = string.Join("", byteArr.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            return byteStr;
        }

        /// <summary>
        /// Adds the length of the input binary string as a prefix to the binary string for deobfuscation purposes.
        /// </summary>
        /// <param name="binaryStr">The input binary string to modify.</param>
        /// <returns>The modified binary string with a length prefix.</returns>
        private static string AddCountToBinaryStr(string binaryStr)
        {
            // Adds the length of the binaryStr for deobfus
            string prefix = binaryStr.Length.ToString() + "!";
            return TextToBinaryStr(prefix) + binaryStr;
        }

        /// <summary>
        /// Modifies a binary string by flipping the bits at specific positions defined by the input salt array.
        /// </summary>
        /// <param name="binaryStr">The input binary string to modify.</param>
        /// <param name="salt">An array of integers representing the bit positions to flip.</param>
        /// <returns>The modified binary string with bits flipped at the specified positions.</returns>
        private static string BinaryStrToSalt(string binaryStr, int[] salt)
        {
            // Flipp bits at salt pos every 8 bits
            StringBuilder modifiedBinaryStr = new StringBuilder(binaryStr);
            foreach (int saltPosition in salt)
                for (int i = saltPosition; i < binaryStr.Length; i += 8)
                    modifiedBinaryStr[i] = (binaryStr[i] == '0') ? '1' : '0';
            binaryStr = modifiedBinaryStr.ToString();
            return binaryStr;
        }

        /// <summary>
        /// Converts a binary string into a black/white bitmap image.
        /// </summary>
        /// <param name="binaryStr">The input binary string to convert.</param>
        /// <param name="outputImagePath">The output image file path.</param>
        /// <returns>A black/white bitmap image.</returns>
        private static Bitmap BinaryStrToImage(string binaryStr, string outputImagePath = null)
        {
            // Calculate Size if not given
            int width = (int)Math.Ceiling(Math.Sqrt(binaryStr.Length));
            int height = width;
            return BinaryStrToImage(binaryStr, width, height, outputImagePath);
        }

        /// <summary>
        /// Converts a binary string into a black/white bitmap image with the specified width and height.
        /// </summary>
        /// <param name="binaryStr">The input binary string to convert.</param>
        /// <param name="width">The width of the output image.</param>
        /// <param name="height">The height of the output image.</param>
        /// <param name="outputImagePath">The output image file path.</param>
        /// <returns>A black/white bitmap image.</returns>
        private static Bitmap BinaryStrToImage(string binaryStr, int width, int height, string outputImagePath = null)
        {
            Bitmap bitmap = new Bitmap(width, height);

            // Lock the bitmap data
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            byte[] bitmapBytes = new byte[bitmapData.Stride * height];
            IntPtr bitmapPtr = bitmapData.Scan0;

            // Fill the bitmap data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = y * width + x;
                    int value = pos < binaryStr.Length ? binaryStr[pos] % 2 : 0;
                    int offset = (y * bitmapData.Stride) + (x * 4);
                    bitmapBytes[offset] = (byte)(value * 255);
                    bitmapBytes[offset + 1] = (byte)(value * 255);
                    bitmapBytes[offset + 2] = (byte)(value * 255);
                    bitmapBytes[offset + 3] = 255;
                }
            }

            // Copy the bitmap data to the bitmap
            Marshal.Copy(bitmapBytes, 0, bitmapPtr, bitmapBytes.Length);

            bitmap.UnlockBits(bitmapData);

            // Save image to MemoryStream
            if (outputImagePath != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    File.WriteAllBytes(outputImagePath + ".png", ms.ToArray());
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Converts a binary string into a list of black/white Bitmap images.
        /// </summary>
        /// <param name="binaryStr">The binary string to convert.</param>
        /// <param name="width">The width of each Bitmap image.</param>
        /// <param name="height">The height of each Bitmap image.</param>
        /// <param name="outputImagePath">The path to save the generated images. Default is null.</param>
        /// <returns>A list of black/white Bitmap images.</returns>
        private static List<Bitmap> BinaryStrToImageList(string binaryStr, int width, int height, string outputImagePath = null)
        {
            // Prepare variable for iteration
            List<Bitmap> bitmapList = new List<Bitmap>();
            int buffer = (width * height / 8) * 8;
            int iterations = (int)Math.Ceiling((double)binaryStr.Length / buffer);

            for (int i = 0; i < iterations; i++)
            {
                // Calculate buffer position foreach image
                int fromPos = i * buffer;
                Bitmap bitmap = new Bitmap(width, height);

                // Lock the bitmap data
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                byte[] bitmapBytes = new byte[bitmapData.Stride * height];
                IntPtr bitmapPtr = bitmapData.Scan0;

                // Fill the bitmap data
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pos = fromPos + (y * width + x);
                        int value;
                        if (pos < binaryStr.Length)
                            value = Convert.ToInt32(binaryStr[pos]) % 2;
                        else
                            value = 0;

                        int offset = (y * bitmapData.Stride) + (x * 4);
                        bitmapBytes[offset] = (byte)(value * 255);
                        bitmapBytes[offset + 1] = (byte)(value * 255);
                        bitmapBytes[offset + 2] = (byte)(value * 255);
                        bitmapBytes[offset + 3] = 255;
                    }
                }

                // Copy the bitmap data to the bitmap
                Marshal.Copy(bitmapBytes, 0, bitmapPtr, bitmapBytes.Length);

                bitmap.UnlockBits(bitmapData);

                // Save image to MemoryStream
                if (outputImagePath != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        File.WriteAllBytes(outputImagePath + $"_{i}.png", ms.ToArray());
                    }
                }

                bitmapList.Add(bitmap);
            }
            return bitmapList;
        }
        #endregion Helper
        #endregion Encoding

        #region Decoding
        #region Text
        /// <summary>
        /// Converts the given image to a text.
        /// </summary>
        /// <param name="bitmap">The image to convert to text.</param>
        /// <returns>A string of text extracted from the image.</returns>
        public static string ImageToText(Bitmap bitmap)
        {
            return TrimTextToOriginalLength(BinaryStrToText(ImageToBinaryStr(bitmap)));
        }

        /// <summary>
        /// Converts the given image to a text, using an array of integers as a salt to desalt the image binary string.
        /// </summary>
        /// <param name="bitmap">The Bitmap object to convert to text.</param>
        /// <param name="salt">The salt array used to decrypt the image.</param>
        /// <returns>The text from the encrypted image.</returns>
        public static string ImageToText(Bitmap bitmap, int[] salt)
        {
            return TrimTextToOriginalLength(BinaryStrToText(DesaltBinaryStr(ImageToBinaryStr(bitmap), salt)));
        }

        /// <summary>
        /// Converts a list of Bitmap images to a text.
        /// </summary>
        /// <param name="bitmapList">The list of Bitmap images to be converted.</param>
        /// <returns>The text extracted from the images.</returns>
        public static string ImageToText(List<Bitmap> bitmapList)
        {
            string result = "";
            foreach (Bitmap bitmap in bitmapList)
                result += BinaryStrToText(ImageToBinaryStr(bitmap));
            return TrimTextToOriginalLength(result);
        }

        /// <summary>
        /// Converts a list of Bitmap images to a text, using an array of integers as a salt to desalt the image binary string.
        /// </summary>
        /// <param name="bitmapList">List of Bitmap objects to be converted to text.</param>
        /// <param name="salt">The salt array used to decrypt the image.</param>
        /// <returns>The text extracted from the encrypted image.</returns>
        public static string ImageToText(List<Bitmap> bitmapList, int[] salt)
        {
            string result = "";
            foreach (Bitmap bitmap in bitmapList)
                result += BinaryStrToText(DesaltBinaryStr(ImageToBinaryStr(bitmap), salt));
            return TrimTextToOriginalLength(result);
        }
        #endregion Text

        #region File
        /// <summary>
        /// Converts the given bitmap image into a file and saves it to the specified output file path.
        /// </summary>
        /// <param name="bitmap">The Bitmap image to be converted.</param>
        /// <param name="outputFilePath">The file path where the converted image will be saved.</param>
        /// <returns>A string indicating the success of the operation.</returns>
        public static string ImageToFile(Bitmap bitmap, string outputFilePath)
        {
            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            // Deobfuse the image
            string binaryStr = ImageToBinaryStr(bitmap);
            int[] indexLenght = GetImageBufferSize(binaryStr);
            BinaryStrToFile(binaryStr.Substring(indexLenght[0], indexLenght[1]), outputFilePath);
            return "Successfully converted to file: " + outputFilePath;
        }

        /// <summary>
        /// Converts the given bitmap image into a file and saves it to the specified output file path, after decrypting it with the given salt array.
        /// </summary>
        /// <param name="bitmap">The Bitmap image to be converted.</param>
        /// <param name="salt">The salt array used to decrypt the image.</param>
        /// <param name="outputFilePath">The file path where the converted image will be saved</param>
        /// <returns>A string indicating the success of the operation.</returns>
        public static string ImageToFile(Bitmap bitmap, int[] salt, string outputFilePath)
        {
            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            // Deobfuse the image
            string binaryStr = DesaltBinaryStr(ImageToBinaryStr(bitmap), salt);
            int[] indexLenght = GetImageBufferSize(binaryStr);
            BinaryStrToFile(binaryStr.Substring(indexLenght[0], indexLenght[1]), outputFilePath);
            return "Successfully converted to file: " + outputFilePath;
        }

        /// <summary>
        /// Converts the given bitmap image list into a file and saves it to the specified output file path.
        /// </summary>
        /// <param name="bitmap">The Bitmap image to be converted.</param>
        /// <param name="outputFilePath">The file path where the converted image will be saved.</param>
        /// <returns>A string indicating the success of the operation.</returns>
        public static string ImageToFile(List<Bitmap> bitmap, string outputFileName)
        {
            ImageToFile(bitmap, null, outputFileName);
            return "Successfully converted to file: " + outputFileName;
        }

        /// <summary>
        /// Converts the given bitmap image list into a file and saves it to the specified output file path, after decrypting it with the given salt array.
        /// </summary>
        /// <param name="bitmap">The Bitmap image to be converted.</param>
        /// <param name="salt">The salt array used to decrypt the image.</param>
        /// <param name="outputFilePath">The file path where the converted image will be saved</param>
        /// <returns>A string indicating the success of the operation.</returns>
        public static string ImageToFile(List<Bitmap> bitmap, int[] salt, string outputFilePath)
        {
            try
            {
                if (File.Exists(outputFilePath))
                    File.Delete(outputFilePath);
                int[] indexLength = { 0, 0 };
                int length = 0;

                // Loop through every image
                for (int i = 0; i < bitmap.Count; i++)
                {
                    // Get the binaryStr
                    string binaryStr;
                    if (salt != null)
                        binaryStr = DesaltBinaryStr(ImageToBinaryStr(bitmap[i]), salt);
                    else
                        binaryStr = ImageToBinaryStr(bitmap[i]);

                    if (i == 0)
                    {
                        // Remove the important informations from the fist image
                        indexLength = GetImageBufferSize(binaryStr);
                        binaryStr = binaryStr.Substring(indexLength[0]);
                        BinaryStrToFile(binaryStr, outputFilePath);
                        length += binaryStr.Length;
                    }
                    else if (i == bitmap.Count - 1)
                    {
                        // Trim the last image to its original size for the file
                        binaryStr = binaryStr.Substring(0, indexLength[1] - length);
                        BinaryStrToFile(binaryStr, outputFilePath);
                    }
                    else
                    {
                        BinaryStrToFile(binaryStr, outputFilePath);
                        length += binaryStr.Length;
                    }
                }
                return "Successfully converted to file: " + outputFilePath;
            }
            catch(ArgumentOutOfRangeException aex)
            {
                return "Error: " + aex.Message;
            }
        }
        #endregion File

        #region Helper
        /// <summary>
        /// Converts a given bitmap image to a binary string representation.
        /// </summary>
        /// <param name="bitmap">The bitmap image to convert.</param>
        /// <returns>A binary string representation of the given bitmap image.</returns>
        private static string ImageToBinaryStr(Bitmap bitmap)
        {
            StringBuilder binaryStr = new StringBuilder();
            int imgArea = bitmap.Width * bitmap.Height;

            // Lock the bitmap data to get faster access to the pixel values
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Get the number of bytes per row in the bitmap
            int bytesPerRow = bmpData.Stride / 4;

            // Process the image row by row
            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bytesPerRow; j++)
                {
                    // Compute the offset of the current pixel in the pixel data
                    int offset = i * bmpData.Stride + j * 4;

                    // Convert the pixel color to binary and append it to the string
                    int color = Color.FromArgb(Marshal.ReadInt32(bmpData.Scan0, offset)).ToArgb();
                    binaryStr.Append((color == Color.Black.ToArgb()) ? "0" : "1");
                }
            }

            // Unlock the bitmap data
            bitmap.UnlockBits(bmpData);

            // Remove unwanted black pixels at the end
            binaryStr.Length = imgArea - (imgArea % 8);
            return binaryStr.ToString();
        }

        /// <summary>
        /// Desalts and flips bits in a binary string using the given salt array.
        /// </summary>
        /// <param name="binaryStr">The binary string to modify.</param>
        /// <param name="salt">An array of integers representing the positions to desalt and flip bits.</param>
        /// <returns>The modified binary string.</returns>
        private static string DesaltBinaryStr(string binaryStr, int[] salt)
        {
            // Desalt and flipp bits
            StringBuilder modifiedBinaryStr = new StringBuilder(binaryStr);
            foreach (int saltPosition in salt)
                for (int i = saltPosition; i < binaryStr.Length; i += 8)
                    modifiedBinaryStr[i] = (binaryStr[i] == '0') ? '1' : '0';
            binaryStr = modifiedBinaryStr.ToString();
            return binaryStr;
        }

        /// <summary>
        /// Converts a binary string into a text string using ASCII encoding.
        /// </summary>
        /// <param name="binaryStr">The binary string to be converted.</param>
        /// <returns>The text string resulting from the conversion.</returns>
        private static string BinaryStrToText(string binaryStr)
        {
            // Convert binaryStr back to text
            byte[] byteArr = new byte[binaryStr.Length / 8];
            for (int i = 0; i < byteArr.Length; i++)
                byteArr[i] = Convert.ToByte(binaryStr.Substring(i * 8, 8), 2);
            string text = Encoding.ASCII.GetString(byteArr);
            return text;
        }

        /// <summary>
        /// Writes the binary string to a file.
        /// </summary>
        /// <param name="binaryStr">The binary string to write to the file.</param>
        /// <param name="outputFilePath">The output file path.</param>
        public static void BinaryStrToFile(string binaryStr, string outputFilePath)
        {
            byte[] buffer = new byte[16384]; // 16 KB buffer
            int bitsRead = 0;
            int bytesWritten = 0;

            using (FileStream fs = new FileStream(outputFilePath, FileMode.Append))
            {
                while (bitsRead < binaryStr.Length)
                {
                    int bitsToRead = Math.Min(8, binaryStr.Length - bitsRead); // Read up to 8 bits at a time
                    string eightBits = binaryStr.Substring(bitsRead, bitsToRead).PadRight(8, '0'); // Pad with zeros if necessary
                    buffer[bytesWritten] = Convert.ToByte(eightBits, 2); // Convert binary string to byte
                    bitsRead += bitsToRead;
                    bytesWritten++;

                    // Write buffer to file when full
                    if (bytesWritten == buffer.Length)
                    {
                        fs.Write(buffer, 0, buffer.Length);
                        bytesWritten = 0;
                    }
                }

                // Write remaining bytes to file
                if (bytesWritten > 0)
                    fs.Write(buffer, 0, bytesWritten);
            }
        }

        /// <summary>
        /// Trims the given text to its original length.
        /// </summary>
        /// <param name="text">The text to trim.</param>
        /// <returns>The trimmed text.</returns>
        private static string TrimTextToOriginalLength(string text)
        {
            // If salt doesnt match it won't get the correct lenght and throw an error
            try
            {
                // Trim Text to original length
                int splitIndex = text.IndexOf('!');
                int length = Convert.ToInt32(text.Substring(0, splitIndex)) / 8;
                text = text.Substring(splitIndex + 1, length);
                return text;
            }
            catch (Exception)
            {
                return "[-] WARNING: Invalid Salt!";
            }
        }

        /// <summary>
        /// Retrieves the length of the encoded image buffer and the starting index of the image data in the binary string.
        /// </summary>
        /// <param name="binaryStr">The binary string containing the encoded image.</param>
        /// <returns>An array of integers, where the first element is the starting index of the image data in the binary string and the second element is the length of the encoded image buffer in bytes.</returns>
        private static int[] GetImageBufferSize(string binaryStr)
        {
            try
            {
                int splitIndex = 0;
                string lenghtStr = "";

                // Loop to find the length
                for (int i = 0; i < binaryStr.Length; i += 8)
                {
                    int charCode = Convert.ToInt32(binaryStr.Substring(i, 8), 2);
                    char myChar = Convert.ToChar(charCode);
                    // Terminator
                    if (myChar == '!')
                    {
                        splitIndex = i;
                        break;
                    }
                    lenghtStr += myChar;
                }

                // Calculate the results
                int length = Convert.ToInt32(lenghtStr);
                int[] result = { splitIndex + 8, length }; // +8 for '!' at the splitIndex
                return result;
            }
            catch (Exception)
            {
                return new int[] { 0, binaryStr.Length };
            }
        }
        #endregion Helper
        #endregion Decoding
    }
}
