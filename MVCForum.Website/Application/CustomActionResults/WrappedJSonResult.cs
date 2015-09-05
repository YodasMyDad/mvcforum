using System.Web.Mvc;

namespace MVCForum.Website.Application
{
    /// This is used to create an html "page" that is dropped into an iframe, as part
    /// of the asynchronous server calls made during a file upload. The JSON data
    /// placed in the page is the result information following processing
    /// of the uploaded file.
    /// Acknowledgement: http://www.dustinhorne.com/post/2011/11/16/AJAX-File-Uploads-with-jQuery-and-MVC-3.aspx
    public class WrappedJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Write("<html><body><textarea id=\"jsonResult\" name=\"jsonResult\">");
            base.ExecuteResult(context);
            context.HttpContext.Response.Write("</textarea></body></html>");
            context.HttpContext.Response.ContentType = "text/html";
        }
    }
}