namespace MvcForum.Web.ViewModels
{
    using System;
    using System.Web;

    public class AttachFileToPostViewModel
    {
        public HttpPostedFileBase[] Files { get; set; }
        public Guid UploadPostId { get; set; }
    }
}