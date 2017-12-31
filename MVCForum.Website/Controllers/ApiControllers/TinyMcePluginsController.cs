namespace MvcForum.Web.Controllers.ApiControllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Core.Models.General;

    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/TinyMce")]
    public class TinyMcePluginsController : ApiController
    {
        //private void SetPrincipal(IPrincipal principal)
        //{
        //    Thread.CurrentPrincipal = principal;
        //    if (HttpContext.Current != null)
        //    {
        //        HttpContext.Current.User = principal;
        //    }
        //}

        //GET api/TinyMce/UploadImage
        [System.Web.Http.Route("UploadImage")]
        [System.Web.Http.HttpPost]
        public string UploadImage()
        {
            var memberService = DependencyResolver.Current.GetService<IMembershipService>();
            var roleService = DependencyResolver.Current.GetService<IRoleService>();
            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            var uploadService = DependencyResolver.Current.GetService<IUploadedFileService>();
            var unitOfWorkManager = DependencyResolver.Current.GetService<IUnitOfWorkManager>();
            var loggingService = DependencyResolver.Current.GetService<ILoggingService>();

            using (var unitOfWork = unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (HttpContext.Current.Request.Files.AllKeys.Any())
                    {
                        // Get the uploaded image from the Files collection
                        var httpPostedFile = HttpContext.Current.Request.Files["file"];
                        if (httpPostedFile != null)
                        {
                            HttpPostedFileBase photo = new HttpPostedFileWrapper(httpPostedFile);
                            var loggedOnReadOnlyUser = memberService.GetUser(HttpContext.Current.User.Identity.Name);
                            var permissions =
                                roleService.GetPermissions(null, loggedOnReadOnlyUser.Roles.FirstOrDefault());
                            // Get the permissions for this category, and check they are allowed to update
                            if (permissions[SiteConstants.Instance.PermissionInsertEditorImages].IsTicked &&
                                loggedOnReadOnlyUser.DisableFileUploads != true)
                            {
                                // woot! User has permission and all seems ok
                                // Before we save anything, check the user already has an upload folder and if not create one
                                var uploadFolderPath = HostingEnvironment.MapPath(
                                    string.Concat(SiteConstants.Instance.UploadFolderPath, loggedOnReadOnlyUser.Id));
                                if (!Directory.Exists(uploadFolderPath))
                                {
                                    Directory.CreateDirectory(uploadFolderPath);
                                }

                                // If successful then upload the file
                                var uploadResult =
                                    AppHelpers.UploadFile(photo, uploadFolderPath, localizationService, true);
                                if (!uploadResult.UploadSuccessful)
                                {
                                    return string.Empty;
                                }

                                // Add the filename to the database
                                var uploadedFile = new UploadedFile
                                {
                                    Filename = uploadResult.UploadedFileName,
                                    MembershipUser = loggedOnReadOnlyUser
                                };
                                uploadService.Add(uploadedFile);

                                // Commit the changes
                                unitOfWork.Commit();

                                return uploadResult.UploadedFileUrl;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    loggingService.Error(ex);
                }
            }

            return string.Empty;
        }
    }
}