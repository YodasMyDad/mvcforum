using System.Web.Mvc;

namespace MVCForum.Website.Application.ActionFilterAttributes
{
    public class NoSlash : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Request.Url != null)
            {
                var originalUrl = filterContext.HttpContext.Request.Url.ToString();
                var newUrl = originalUrl.TrimEnd('/');
                if (originalUrl.Length != newUrl.Length)
                {
                    filterContext.HttpContext.Response.Redirect(newUrl);
                }                    
            }
        }

        //===== Usage
        //[NoSlash] <<
        //[Route("robots.txt")]
        //public async Task<ActionResult> Robots()
        //{
        //    string robots = getRobotsContent();
        //    return Content(robots, "text/plain");
        //}

    }
}