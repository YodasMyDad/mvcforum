using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Website.Application.RouteHandlers
{
    public class SlugRouteHandler : MvcRouteHandler
    {
        private readonly ITopicService _topicService;
        public SlugRouteHandler()
        {
            _topicService = ServiceFactory.Get<ITopicService>();
        }

        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var url = requestContext.HttpContext.Request.Path.TrimStart('/');

            if (!string.IsNullOrEmpty(url))
            {
                // See if there is a topic by slug
                FillRequest("Topic", "Show", url, requestContext);

                //PageItem page = RedirectManager.GetPageByFriendlyUrl(url);
                //if (page != null)
                //{
                //    FillRequest(page.ControllerName,
                //        page.ActionName ?? "GetStatic",
                //        page.ID.ToString(),
                //        requestContext);
                //}

                
            }

            return base.GetHttpHandler(requestContext);
        }

        private static void FillRequest(string controller, string action, string slug, RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            requestContext.RouteData.Values["controller"] = controller;
            requestContext.RouteData.Values["action"] = action;
            requestContext.RouteData.Values["slug"] = slug;
        }
    }
}
