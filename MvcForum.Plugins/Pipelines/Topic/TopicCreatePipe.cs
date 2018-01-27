namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class TopicCreatePipe : IPipe<IPipelineProcess<Topic>>
    {
        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // TODO - This is actually part of the post pipeline
            //if (topicViewModel.Files != null)
            //{
            //    // Get the permissions for this category, and check they are allowed to update
            //    if (permissions[ForumConfiguration.Instance.PermissionAttachFiles].IsTicked &&
            //        loggedOnReadOnlyUser.DisableFileUploads != true)
            //    {
            //        // woot! User has permission and all seems ok
            //        // Before we save anything, check the user already has an upload folder and if not create one
            //        var uploadFolderPath =
            //            HostingEnvironment.MapPath(string.Concat(
            //                ForumConfiguration.Instance.UploadFolderPath,
            //                loggedOnReadOnlyUser.Id));
            //        if (!Directory.Exists(uploadFolderPath))
            //        {
            //            Directory.CreateDirectory(uploadFolderPath);
            //        }

            //        // Loop through each file and get the file info and save to the users folder and Db
            //        foreach (var file in topicViewModel.Files)
            //        {
            //            if (file != null)
            //            {
            //                // If successful then upload the file
            //                var uploadResult = file.UploadFile(uploadFolderPath, LocalizationService);
            //                if (!uploadResult.UploadSuccessful)
            //                {
            //                    TempData[Constants.MessageViewBagName] =
            //                        new GenericMessageViewModel
            //                        {
            //                            Message = uploadResult.ErrorMessage,
            //                            MessageType = GenericMessages.danger
            //                        };
            //                    Context.RollBack();
            //                    return View(topicViewModel);
            //                }

            //                // Add the filename to the database
            //                var uploadedFile = new UploadedFile
            //                {
            //                    Filename = uploadResult.UploadedFileName,
            //                    Post = topicPost,
            //                    MembershipUser = loggedOnUser
            //                };
            //                _uploadedFileService.Add(uploadedFile);
            //            }
            //        }
            //    }
            //}

            // TODO - Run post pipline

            // TODO - Add post to topic and make it last post

            throw new NotImplementedException();
        }
    }
}