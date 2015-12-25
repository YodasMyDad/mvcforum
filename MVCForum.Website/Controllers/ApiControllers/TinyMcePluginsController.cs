using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Domain.Constants;

namespace MVCForum.Website.Controllers.ApiControllers
{
    [Authorize]
    [RoutePrefix("api/TinyMce")]
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
        [Route("UploadImage")]
        [HttpPost]
        public string UploadImage()
        {
            var memberService = ServiceFactory.Get<IMembershipService>();
            var roleService = ServiceFactory.Get<IRoleService>();
            var localizationService = ServiceFactory.Get<ILocalizationService>();
            var uploadService = ServiceFactory.Get<IUploadedFileService>();
            var unitOfWorkManager = ServiceFactory.Get<IUnitOfWorkManager>();
            var loggingService = ServiceFactory.Get<ILoggingService>();

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
                            var permissions = roleService.GetPermissions(null, loggedOnReadOnlyUser.Roles.FirstOrDefault());
                            // Get the permissions for this category, and check they are allowed to update
                            if (permissions[SiteConstants.Instance.PermissionInsertEditorImages].IsTicked && loggedOnReadOnlyUser.DisableFileUploads != true)
                            {
                                // woot! User has permission and all seems ok
                                // Before we save anything, check the user already has an upload folder and if not create one
                                var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, loggedOnReadOnlyUser.Id));
                                if (!Directory.Exists(uploadFolderPath))
                                {
                                    Directory.CreateDirectory(uploadFolderPath);
                                }

                                // If successful then upload the file
                                var uploadResult = AppHelpers.UploadFile(photo, uploadFolderPath, localizationService, true);
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

