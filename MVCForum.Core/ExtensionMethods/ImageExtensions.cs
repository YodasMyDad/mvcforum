namespace MvcForum.Core.ExtensionMethods
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Models.General;
    using Providers.Storage;

    public static class ImageExtensions
    {
        /// <summary>
        ///     Convert an image to base64 string
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string ImageToBase64(this Image image)
        {
            using (var m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                var imageBytes = m.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        /// <summary>
        ///     Converts a BAse 64 string back into an image
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static Image Base64ToImage(this string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            Image image;
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);
                image = Image.FromStream(ms, true);
            }
            return image;
        }


        /// <summary>
        /// Turns a posted image to a C# image and rotates as needed
        /// </summary>
        /// <param name="httpPostedFile"></param>
        /// <returns></returns>
        public static Image ToImage(this HttpPostedFileBase httpPostedFile)
        {
            // Rotate image if wrong want around
            var sourceimage = Image.FromStream(httpPostedFile.InputStream, true, true);

            if (sourceimage.PropertyIdList.Contains(0x0112))
            {
                int rotationValue = sourceimage.GetPropertyItem(0x0112).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;

                    case 8: // rotated 90 right
                            // de-rotate:
                        sourceimage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;

                    case 3: // bottoms up
                        sourceimage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;

                    case 6: // rotated 90 left
                        sourceimage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                }
            }

            return sourceimage;
        }

        /// <summary>
        /// Uploads an Image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="fileName"></param>
        /// <param name="uploadFolderPath"></param>
        /// <returns></returns>
        public static UploadFileResult Upload(this Image image, string uploadFolderPath, string fileName)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                // If no file name make one
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = "I".AppendUniqueIdentifier();
                    fileName = $"{fileName.ToLower()}.jpg";
                }

                using (var stream = new MemoryStream())
                {
                    // Save the image as a Jpeg only
                    var bmp = new Bitmap(image);
                    bmp.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;

                    var file = new MemoryFile(stream, "image/jpeg", fileName);

                    // Get the storage provider and save file
                    upResult.UploadedFileName = fileName;
                    upResult.UploadedFileUrl = StorageProvider.Current.SaveAs(uploadFolderPath, fileName, file);
                }
            }
            catch (Exception ex)
            {
                upResult.UploadSuccessful = false;
                upResult.ErrorMessage = ex.Message;
            }

            return upResult;
        }

    }
}