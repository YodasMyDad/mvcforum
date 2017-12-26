namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;

    public class FileController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IUploadedFileService _uploadedFileService;

        public FileController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IUploadedFileService uploadedFileService, ICategoryService categoryService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService,
                settingsService, cacheService)
        {
            _uploadedFileService = uploadedFileService;
            _categoryService = categoryService;
        }

        public FileResult Download(Guid id)
        {
            var uploadedFileById = _uploadedFileService.Get(id);
            if (uploadedFileById != null)
            {
                // Check the user has permission to download this file
                var fileCategory = uploadedFileById.Post.Topic.Category;
                var allowedCategoryIds = _categoryService.GetAllowedCategories(UsersRole).Select(x => x.Id);
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

        public PartialViewResult ImageUploadTinyMce()
        {
            // Testing
            return PartialView();
        }
    }
}