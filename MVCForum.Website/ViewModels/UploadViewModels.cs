using System;
using System.Web;

namespace MVCForum.Website.ViewModels
{
    public class AttachFileToPostViewModel
    {
        public HttpPostedFileBase[] Files { get; set; }
        public Guid UploadPostId { get; set; }
    }
}