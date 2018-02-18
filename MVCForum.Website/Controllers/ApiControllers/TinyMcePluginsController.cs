namespace MvcForum.Web.Controllers.ApiControllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;
    using Core;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Ioc;
    using Core.Models.General;
    using Unity;

    [Authorize]
    [RoutePrefix("api/TinyMce")]
    public partial class TinyMcePluginsController : ApiController
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
        public virtual string UploadImage()
        {
            var memberService = UnityHelper.Container.Resolve<IMembershipService>();
            var roleService = UnityHelper.Container.Resolve<IRoleService>();
            var localizationService = UnityHelper.Container.Resolve<ILocalizationService>();
            var uploadService = UnityHelper.Container.Resolve<IUploadedFileService>();
            var context = UnityHelper.Container.Resolve<IMvcForumContext>();
            var loggingService = UnityHelper.Container.Resolve<ILoggingService>();

            roleService.RefreshContext(context);
            uploadService.RefreshContext(context);
            localizationService.RefreshContext(context);
            memberService.RefreshContext(context);

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
                        if (permissions[ForumConfiguration.Instance.PermissionInsertEditorImages].IsTicked &&
                            loggedOnReadOnlyUser.DisableFileUploads != true)
                        {
                            // woot! User has permission and all seems ok
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = HostingEnvironment.MapPath(
                                string.Concat(ForumConfiguration.Instance.UploadFolderPath, loggedOnReadOnlyUser.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // If successful then upload the file
                            var uploadResult = photo.UploadFile(uploadFolderPath, localizationService, true);
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
                            context.SaveChanges();

                            return uploadResult.UploadedFileUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.RollBack();
                loggingService.Error(ex);
            }


            return string.Empty;
        }
    }
}