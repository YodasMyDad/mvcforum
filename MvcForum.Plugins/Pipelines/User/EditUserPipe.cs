namespace MvcForum.Plugins.Pipelines.User
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class EditUserPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IMembershipService _membershipService;
        private readonly ILocalizationService _localizationService;

        public EditUserPipe(IMembershipService membershipService, ILocalizationService localizationService)
        {
            _membershipService = membershipService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            // Grab the logged in user (This should always be there)
            var principal = input.ExtendedData.GetExtendedDataItem<IPrincipal>(Constants.ExtendedDataKeys.UserObject);
            var loggedOnUser = principal.GetMembershipUser(_membershipService);

            // Grab out the image if we have one
            if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.ImageBase64))
            {
                var avatar = input.ExtendedData[Constants.ExtendedDataKeys.ImageBase64].Base64ToImage();

                // Check we're good
                if (avatar != null)
                {
                    // Before we save anything, check the user already has an upload folder and if not create one
                    var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(ForumConfiguration.Instance.UploadFolderPath, loggedOnUser.Id));


                    // If successful then upload the file
                    // Only downside to doing it this want is that 
                    var uploadResult = avatar.Upload(uploadFolderPath, string.Empty);

                    //if (!uploadResult.UploadSuccessful)
                    //{
                    //    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    //    {
                    //        Message = uploadResult.ErrorMessage,
                    //        MessageType = GenericMessages.danger
                    //    };
                    //    return View(userModel);
                    //}
                }
            }            

            return input;
        }
    }
}