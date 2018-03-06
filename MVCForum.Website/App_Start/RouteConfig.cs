namespace MvcForum.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;
    using Core;
    using Core.Constants;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            RouteTable.Routes.LowercaseUrls = true;
            RouteTable.Routes.AppendTrailingSlash = true;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new {favicon = @"(.*/)?favicon.ico(/.*)?"});

            // API Attribute Routes
            //routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                "categoryUrls", // Route name
                string.Concat(ForumConfiguration.Instance.CategoryUrlIdentifier, "/{slug}"), // URL with parameters
                new {controller = "Category", action = "Show", slug = UrlParameter.Optional} // Parameter defaults
            );

            routes.MapRoute(
                "categoryRssUrls", // Route name
                string.Concat(ForumConfiguration.Instance.CategoryUrlIdentifier, "/rss/{slug}"), // URL with parameters
                new
                {
                    controller = "Category",
                    action = "CategoryRss",
                    slug = UrlParameter.Optional
                } // Parameter defaults
            );

            routes.MapRoute(
                "topicUrls", // Route name
                string.Concat(ForumConfiguration.Instance.TopicUrlIdentifier, "/{slug}"), // URL with parameters
                new {controller = "Topic", action = "Show", slug = UrlParameter.Optional} // Parameter defaults
            );

            routes.MapRoute(
                "memberUrls", // Route name
                string.Concat(ForumConfiguration.Instance.MemberUrlIdentifier, "/{slug}"), // URL with parameters
                new {controller = "Members", action = "GetByName", slug = UrlParameter.Optional} // Parameter defaults
            );

            routes.MapRoute(
                "tagUrls", // Route name
                string.Concat(ForumConfiguration.Instance.TagsUrlIdentifier, "/{tag}"), // URL with parameters
                new {controller = "Topic", action = "TopicsByTag", tag = UrlParameter.Optional} // Parameter defaults
            );

            routes.MapRoute(
                "topicXmlSitemap", // Route name
                "topicxmlsitemap", // URL with parameters
                new {controller = "Home", action = "GoogleSitemap"} // Parameter defaults
            );

            routes.MapRoute(
                "categoryXmlSitemap", // Route name
                "categoryxmlsitemap", // URL with parameters
                new {controller = "Home", action = "GoogleCategorySitemap"} // Parameter defaults
            );

            routes.MapRoute(
                "memberXmlSitemap", // Route name
                "memberxmlsitemap", // URL with parameters
                new {controller = "Home", action = "GoogleMemberSitemap"} // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
            );
            //.RouteHandler = new SlugRouteHandler()
        }
    }
}