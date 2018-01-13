using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcForum.Core.ExtensionMethods
{
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web;
    using Constants;
    using Interfaces.Services;
    using Models.General;
    using Providers.Storage;
    using Utilities;

    public static class HttpExtensionMethods
    {
        /// <summary>
        /// Uploads a file from a posted file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="uploadFolderPath"></param>
        /// <param name="localizationService"></param>
        /// <param name="onlyImages"></param>
        /// <returns></returns>
        public static UploadFileResult UploadFile(this HttpPostedFileBase file, string uploadFolderPath,
                                ILocalizationService localizationService, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };
            const string imageExtensions = "jpg,jpeg,png,gif";
            var fileName = Path.GetFileName(file.FileName);
            var storageProvider = StorageProvider.Current;

            if (fileName != null)
            {
                // Lower case
                fileName = fileName.ToLower();

                // Get the file extension
                var fileExtension = Path.GetExtension(fileName);

                //Before we do anything, check file size
                if (file.ContentLength > Convert.ToInt32(ForumConfiguration.Instance.FileUploadMaximumFileSizeInBytes))
                {
                    //File is too big
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = localizationService.GetResourceString("Post.UploadFileTooBig");
                    return upResult;
                }

                // now check allowed extensions
                var allowedFileExtensions = ForumConfiguration.Instance.FileUploadAllowedExtensions;

                if (onlyImages)
                {
                    allowedFileExtensions = imageExtensions;
                }

                if (!string.IsNullOrWhiteSpace(allowedFileExtensions))
                {
                    // Turn into a list and strip unwanted commas as we don't trust users!
                    var allowedFileExtensionsList = allowedFileExtensions.ToLower().Trim()
                        .TrimStart(',').TrimEnd(',').Split(',').ToList();

                    // If can't work out extension then just error
                    if (string.IsNullOrWhiteSpace(fileExtension))
                    {
                        upResult.UploadSuccessful = false;
                        upResult.ErrorMessage = localizationService.GetResourceString("Errors.GenericMessage");
                        return upResult;
                    }

                    // Remove the dot then check against the extensions in the web.config settings
                    fileExtension = fileExtension.TrimStart('.');
                    if (!allowedFileExtensionsList.Contains(fileExtension))
                    {
                        upResult.UploadSuccessful = false;
                        upResult.ErrorMessage = localizationService.GetResourceString("Post.UploadBannedFileExtension");
                        return upResult;
                    }
                }

                // Store these here as we may change the values within the image manipulation
                var newFileName = string.Empty;
                var path = string.Empty;

                if (imageExtensions.Split(',').ToList().Contains(fileExtension))
                {
                    // Rotate image if wrong want around
                    using (var sourceimage = Image.FromStream(file.InputStream))
                    {
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

                        using (var stream = new MemoryStream())
                        {
                            // Save the image as a Jpeg only
                            sourceimage.Save(stream, ImageFormat.Jpeg);
                            stream.Position = 0;

                            // Change the extension to jpg as that's what we are saving it as
                            fileName = fileName.Replace(fileExtension, "");
                            fileName = string.Concat(fileName, "jpg");
                            file = new MemoryFile(stream, "image/jpeg", fileName);

                            // Sort the file name
                            newFileName = CreateNewFileName(fileName);

                            // Get the storage provider and save file
                            upResult.UploadedFileUrl = storageProvider.SaveAs(uploadFolderPath, newFileName, file);
                        }
                    }
                }
                else
                {
                    // Sort the file name
                    newFileName = CreateNewFileName(fileName);
                    upResult.UploadedFileUrl = storageProvider.SaveAs(uploadFolderPath, newFileName, file);
                }

                upResult.UploadedFileName = newFileName;
            }

            return upResult;
        }

        private static string CreateNewFileName(string fileName)
        {
            return $"{GuidComb.GenerateComb()}_{fileName.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower()}";
        }
    }
}
