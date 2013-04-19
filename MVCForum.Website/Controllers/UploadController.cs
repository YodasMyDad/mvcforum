using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public class UploadController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IUploadedFileService _uploadedFileService;

        private readonly MembershipUser LoggedOnUser;
        private readonly MembershipRole UsersRole;

        public UploadController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, 
            IPostService postService, IUploadedFileService uploadedFileService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _uploadedFileService = uploadedFileService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }

        [HttpPost]
        public ActionResult UploadPostFiles(AttachFileToPostViewModel attachFileToPostViewModel)
        {

            if (attachFileToPostViewModel != null && attachFileToPostViewModel.Files != null)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // First this to do is get the post
                    var post = _postService.Get(attachFileToPostViewModel.UploadPostId);

                    // Check we get a valid post back and have some file
                    if (post != null && attachFileToPostViewModel.Files != null)
                    {
                        Topic topic = null;
                        try
                        {
                            // Now get the topic
                            topic = post.Topic;

                            // Now get the category
                            var category = topic.Category;

                            // Get the permissions for this category, and check they are allowed to update and 
                            // not trying to be a sneaky mofo
                            var permissions = RoleService.GetPermissions(category, UsersRole);
                            if (permissions[AppConstants.PermissionAttachFiles].IsTicked == false)
                            {
                                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
                            }

                            // woot! User has permission and all seems ok
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = Server.MapPath(string.Concat(AppConstants.UploadFolderPath, LoggedOnUser.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // Loop through each file and get the file info and save to the users folder and Db
                            foreach (var file in attachFileToPostViewModel.Files)
                            {
                                if (file != null)
                                {
                                    var fileName = Path.GetFileName(file.FileName);
                                    if (fileName != null)
                                    {                   
                                        //Before we do anything, check file size
                                        if (file.ContentLength > Convert.ToInt32(ConfigUtils.GetAppSetting("FileUploadMaximumFileSizeInBytes")))
                                        {
                                            //File is too big
                                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                            {
                                                Message = LocalizationService.GetResourceString("Post.UploadFileTooBig"),
                                                MessageType = GenericMessages.error
                                            };
                                            return Redirect(topic.NiceUrl);
                                        }

                                        // now check allowed extensions
                                        var allowedFileExtensions = ConfigUtils.GetAppSetting("FileUploadAllowedExtensions");
                                        if (!string.IsNullOrEmpty(allowedFileExtensions))
                                        {
                                            // Turn into a list and strip unwanted commas as we don't trust users!
                                            var allowedFileExtensionsList = allowedFileExtensions.ToLower().Trim()
                                                                             .TrimStart(',').TrimEnd(',').Split(',').ToList();

                                            // Get the file extension
                                            var fileExtension = Path.GetExtension(file.FileName.ToLower());

                                            // If can't work out extension then just error
                                            if (string.IsNullOrEmpty(fileExtension))
                                            {
                                                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
                                            }

                                            // Remove the dot then check against the extensions in the web.config settings
                                            fileExtension = fileExtension.TrimStart('.');
                                            if (!allowedFileExtensionsList.Contains(fileExtension))
                                            {
                                                // File extension now allowed
                                                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                {
                                                    Message = LocalizationService.GetResourceString("Post.UploadBannedFileExtension"),
                                                    MessageType = GenericMessages.error
                                                };
                                                return Redirect(topic.NiceUrl);
                                            }
                                        }


                                        // Sort the file name
                                        var newFileName = string.Format("{0}_{1}", GuidComb.GenerateComb(), file.FileName.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower());
                                        var path = Path.Combine(uploadFolderPath, newFileName);

                                        // Save the file to disk
                                        file.SaveAs(path);

                                        // Add the filename to the database
                                        var uploadedFile = new UploadedFile
                                            {
                                                Filename = newFileName,
                                                Post = post,
                                                MembershipUser = LoggedOnUser
                                            };
                                        _uploadedFileService.Add(uploadedFile);
                                    }
                                }
                            }

                            //Commit
                            unitOfWork.Commit();

                            // Redirect to the topic with a success message
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Post.FilesUploaded"),
                                MessageType = GenericMessages.success
                            };

                            return Redirect(topic.NiceUrl);
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                                MessageType = GenericMessages.error
                            };
                            return topic != null ? Redirect(topic.NiceUrl) : ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
                        }

                    } 
                }

            }

            // Else return with error to home page
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

    }
}
