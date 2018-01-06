using Microsoft.Owin;
using Owin;
using Hangfire;

[assembly: OwinStartup(typeof(MvcForum.Web.Startup))]
namespace MvcForum.Web
{
    using Core.Constants;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(SiteConstants.Instance.MvcForumContext);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}