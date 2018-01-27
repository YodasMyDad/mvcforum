namespace MvcForum.Plugins.Pipelines.Post
{
    using System.Data.Entity;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.General;

    public class CreateNewPostPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly ILocalizationService _localizationService;

        public CreateNewPostPipe(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            // Files
            if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
            {
                // Get the files
                if (input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] is HttpPostedFileBase[] files)
                {
                    // Username
                    var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

                    // Loggedonuser
                    var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

                    // Before we save anything, check the user already has an upload folder and if not create one
                    var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(ForumConfiguration.Instance.UploadFolderPath, loggedOnUser.Id));
                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    // Loop through each file and get the file info and save to the users folder and Db
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            // If successful then upload the file
                            var uploadResult = file.UploadFile(uploadFolderPath, _localizationService);
                            if (!uploadResult.UploadSuccessful)
                            {
                                //TempData[Constants.MessageViewBagName] =
                                //    new GenericMessageViewModel
                                //    {
                                //        Message = uploadResult.ErrorMessage,
                                //        MessageType = GenericMessages.danger
                                //    };
                                //Context.RollBack();
                                //return View(topicViewModel);
                            }

                            // Add the filename to the database
                            var uploadedFile = new UploadedFile
                            {
                                Filename = uploadResult.UploadedFileName,
                                Post = input.EntityToProcess,
                                MembershipUser = loggedOnUser
                            };
                            _uploadedFileService.Add(uploadedFile);
                        }
                    }
                }
            }

            // Now save
            var saved = await context.SaveChangesAsync();
            if (saved <= 0)
            {
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                return input;
            }

            input.ProcessLog.Add("Post created successfully");
            return input;
        }
    }
}