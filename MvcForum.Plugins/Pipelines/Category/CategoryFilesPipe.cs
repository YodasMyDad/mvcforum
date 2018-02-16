namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
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

    public class CategoryFilesPipe : IPipe<IPipelineProcess<Category>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;

        public CategoryFilesPipe(ILocalizationService localizationService, ILoggingService loggingService)
        {
            _localizationService = localizationService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input,
            IMvcForumContext context)
        {
            _localizationService.RefreshContext(context);

            try
            {
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
                {
                    // Sort image out first
                    if (input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] is HttpPostedFileBase[] files)
                    {
                        // Before we save anything, check the user already has an upload folder and if not create one
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(ForumConfiguration.Instance.UploadFolderPath, input.EntityToProcess.Id));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath ?? throw new InvalidOperationException());
                        }

                        // Loop through each file and get the file info and save to the users folder and Db
                        var file = files[0];
                        if (file != null)
                        {
                            // If successful then upload the file
                            var uploadResult = file.UploadFile(uploadFolderPath, _localizationService, true);

                            if (!uploadResult.UploadSuccessful)
                            {
                                input.AddError(uploadResult.ErrorMessage);
                                return input;
                            }

                            // Save avatar to user
                            input.EntityToProcess.Image = uploadResult.UploadedFileName;

                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}