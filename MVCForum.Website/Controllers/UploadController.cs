namespace MvcForum.Web.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.General;
    using ViewModels;

    [Authorize]
    public partial class UploadController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IUploadedFileService _uploadedFileService;

        public UploadController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPostService postService, IUploadedFileService uploadedFileService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _postService = postService;
            _uploadedFileService = uploadedFileService;
        }

        [HttpPost]
        public virtual ActionResult UploadPostFiles(AttachFileToPostViewModel attachFileToPostViewModel)
        {
            var topic = new Topic();

            try
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // First this to do is get the post
                var post = _postService.Get(attachFileToPostViewModel.UploadPostId);
                if (post != null)
                {
                    // Now get the topic
                    topic = post.Topic;

                    // Check we get a valid post back and have some file
                    if (attachFileToPostViewModel.Files != null && attachFileToPostViewModel.Files.Any())
                    {
                        // Now get the category
                        var category = topic.Category;

                        // Get the permissions for this category, and check they are allowed to update and 
                        // not trying to be a sneaky mofo
                        var permissions = RoleService.GetPermissions(category, loggedOnUsersRole);
                        if (permissions[ForumConfiguration.Instance.PermissionAttachFiles].IsTicked == false ||
                            loggedOnReadOnlyUser.DisableFileUploads == true)
                        {
                            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                                MessageType = GenericMessages.danger
                            };

                            return Redirect(topic.NiceUrl);
                        }

                        // woot! User has permission and all seems ok
                        // Before we save anything, check the user already has an upload folder and if not create one
                        var uploadFolderPath = HostingEnvironment.MapPath(
                            string.Concat(ForumConfiguration.Instance.UploadFolderPath, loggedOnReadOnlyUser.Id));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        // Loop through each file and get the file info and save to the users folder and Db
                        foreach (var file in attachFileToPostViewModel.Files)
                        {
                            if (file != null)
                            {
                                // If successful then upload the file
                                var uploadResult = file.UploadFile(uploadFolderPath, LocalizationService);
                                if (!uploadResult.UploadSuccessful)
                                {
                                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = uploadResult.ErrorMessage,
                                        MessageType = GenericMessages.danger
                                    };
                                    return Redirect(topic.NiceUrl);
                                }

                                // Add the filename to the database
                                var loggedOnUser = MembershipService.GetUser(loggedOnReadOnlyUser.Id);
                                var uploadedFile = new UploadedFile
                                {
                                    Filename = uploadResult.UploadedFileName,
                                    Post = post,
                                    MembershipUser = loggedOnUser
                                };
                                _uploadedFileService.Add(uploadedFile);
                            }
                        }

                        //Commit
                        Context.SaveChanges();

                        // Redirect to the topic with a success message
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Post.FilesUploaded"),
                            MessageType = GenericMessages.success
                        };

                        return Redirect(topic.NiceUrl);
                    }
                    // Else return with error to home page
                    return topic != null
                        ? Redirect(topic.NiceUrl)
                        : ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                // Else return with error to home page
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                    MessageType = GenericMessages.danger
                };
                return topic != null
                    ? Redirect(topic.NiceUrl)
                    : ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
        }

        public virtual ActionResult DeleteUploadedFile(Guid id)
        {
            if (id != Guid.Empty)
            {
                Topic topic = null;
                try
                {
                    var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                    var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                    // Get the file and associated objects we'll need
                    var uploadedFile = _uploadedFileService.Get(id);
                    var post = uploadedFile.Post;
                    topic = post.Topic;

                    if (loggedOnUsersRole.RoleName == Constants.AdminRoleName ||
                        uploadedFile.MembershipUser.Id == loggedOnReadOnlyUser.Id)
                    {
                        // Ok to delete file
                        // Remove it from the post
                        post.Files.Remove(uploadedFile);

                        // store the file path as we'll need it to delete on the file system
                        var filePath = uploadedFile.FilePath;

                        // Now delete it
                        _uploadedFileService.Delete(uploadedFile);


                        // And finally delete from the file system
                        System.IO.File.Delete(HostingEnvironment.MapPath(filePath));
                    }
                    else
                    {
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                            MessageType = GenericMessages.danger
                        };
                        Redirect(topic.NiceUrl);
                    }

                    //Commit
                    Context.SaveChanges();

                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("File.SuccessfullyDeleted"),
                        MessageType = GenericMessages.success
                    };
                    return Redirect(topic.NiceUrl);
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                        MessageType = GenericMessages.danger
                    };
                    return topic != null
                        ? Redirect(topic.NiceUrl)
                        : ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }
    }
}