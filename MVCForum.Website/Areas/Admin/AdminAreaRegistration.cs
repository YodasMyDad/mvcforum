using System.Web.Mvc;
using System.Web.Routing;

namespace MvcForum.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            RouteTable.Routes.LowercaseUrls = true;
            RouteTable.Routes.AppendTrailingSlash = true;

            //context.MapRoute(
            //    "Admin_editcategoryroute",
            //    "Admin/{controller}/{action}/{id}",
            //    new { controller = "AdminCategory", action = "Index", id = UrlParameter.Optional }
            //);
            //context.MapRoute(
            //    "Admin_edituserroute",
            //    "Admin/{controller}/{action}/{userId}",
            //    new { controller = "Admin", action = "Index", userId = UrlParameter.Optional }
            //);
            //context.MapRoute(
            //    "Admin_pagingroute",
            //    "Admin/{controller}/{action}/{page}",
            //    new { controller = "Account", action = "Index", page = UrlParameter.Optional }
            //);
            context.MapRoute(
                "Admin_defaultroute",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
