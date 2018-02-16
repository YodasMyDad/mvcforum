namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;

    public partial class FileController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IUploadedFileService _uploadedFileService;

        public FileController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IUploadedFileService uploadedFileService, ICategoryService categoryService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _uploadedFileService = uploadedFileService;
            _categoryService = categoryService;
        }

        public virtual FileResult Download(Guid id)
        {
            var uploadedFileById = _uploadedFileService.Get(id);
            if (uploadedFileById != null)
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // Check the user has permission to download this file
                var fileCategory = uploadedFileById.Post.Topic.Category;
                var allowedCategoryIds = _categoryService.GetAllowedCategories(loggedOnUsersRole).Select(x => x.Id);
                if (allowedCategoryIds.Contains(fileCategory.Id))
                {
                    //if(AppHelpers.FileIsImage(uploadedFileById.FilePath))
                    //{

                    //}

                    var fileBytes = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath(uploadedFileById.FilePath));
                    return File(fileBytes, MediaTypeNames.Application.Octet, uploadedFileById.Filename);
                }
            }
            return null;
        }

        public virtual PartialViewResult ImageUploadTinyMce()
        {
            // Testing
            return PartialView();
        }
    }
}