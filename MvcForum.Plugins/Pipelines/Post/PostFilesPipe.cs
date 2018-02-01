namespace MvcForum.Plugins.Pipelines.Post
{
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
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

    public class PostFilesPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly ILoggingService _loggingService;

        public PostFilesPipe(ILocalizationService localizationService, IUploadedFileService uploadedFileService, ILoggingService loggingService)
        {
            _localizationService = localizationService;
            _uploadedFileService = uploadedFileService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            _localizationService.RefreshContext(context);
            _uploadedFileService.RefreshContext(context);

            try
            {
                // Files
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
                {
                    // Get the files
                    if (input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] is HttpPostedFileBase[] files)
                    {
                        if (files.Any(x => x != null))
                        {
                            // Username
                            var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

                            // Loggedonuser
                            var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(ForumConfiguration.Instance.UploadFolderPath,
                                                                                    loggedOnUser.Id));
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
                                        input.AddError(uploadResult.ErrorMessage);
                                        return input;
                                    }

                                    // Add the filename to the database
                                    var uploadedFile = new UploadedFile
                                    {
                                        Filename = uploadResult.UploadedFileName,
                                        Post = input.EntityToProcess,
                                        MembershipUser = input.EntityToProcess.User
                                    };

                                    _uploadedFileService.Add(uploadedFile);
                                }
                            }

                            // Was the post successful
                            if (await context.SaveChangesAsync() <= 0)
                            {
                                // Problem
                                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}