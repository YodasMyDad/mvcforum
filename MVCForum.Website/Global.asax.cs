using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EFCachingProvider;
using EFCachingProvider.Caching;
using EFCachingProvider.Web;
using LowercaseRoutesMVC;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;

namespace MVCForum.Website
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {

        public IUnitOfWorkManager UnitOfWorkManager
        {
            get { return DependencyResolver.Current.GetService<IUnitOfWorkManager>(); }
        }

        public IBadgeService BadgeService
        {
            get { return DependencyResolver.Current.GetService<IBadgeService>(); }
        }

        public ISettingsService SettingsService
        {
            get { return DependencyResolver.Current.GetService<ISettingsService>(); }
        }

        public ILoggingService LoggingService
        {
            get { return DependencyResolver.Current.GetService<ILoggingService>(); }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRouteLowercase(
                "categoryUrls", // Route name
                string.Concat(AppConstants.CategoryUrlIdentifier, "/{slug}"), // URL with parameters
                new { controller = "Category", action = "Show", slug = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRouteLowercase(
                "categoryRssUrls", // Route name
                string.Concat(AppConstants.CategoryUrlIdentifier, "/rss/{slug}"), // URL with parameters
                new { controller = "Category", action = "CategoryRss", slug = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRouteLowercase(
                "topicUrls", // Route name
                string.Concat(AppConstants.TopicUrlIdentifier, "/{slug}"), // URL with parameters
                new { controller = "Topic", action = "Show", slug = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRouteLowercase(
                "memberUrls", // Route name
                string.Concat(AppConstants.MemberUrlIdentifier, "/{slug}"), // URL with parameters
                new { controller = "Members", action = "GetByName", slug = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRouteLowercase(
                "tagUrls", // Route name
                string.Concat(AppConstants.TagsUrlIdentifier, "/{tag}"), // URL with parameters
                new { controller = "Topic", action = "TopicsByTag", tag = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRouteLowercase(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            // Register routes
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            Application["Version"] = string.Format("{0}.{1}", version.Major, version.Minor);            

            // Now check the version in the web.config
            var currentVersion = ConfigUtils.GetAppSetting("MVCForumVersion");

            // If the versions are different kick the installer into play
            if (currentVersion == Application["Version"].ToString())
            {
                // If the same carry on as normal
                LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
                LoggingService.Error("START APP");

                // Set the view engine
                ViewEngines.Engines.Clear();
                ViewEngines.Engines.Add(new ForumViewEngine(SettingsService));

                // Set up the EF Caching provider
                EFCachingProviderConfiguration.DefaultCache = new AspNetCache();
                EFCachingProviderConfiguration.DefaultCachingPolicy = CachingPolicy.CacheAll;

                // Do the badge processing
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        BadgeService.SyncBadges();
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(string.Format("Error processing badge classes: {0}", ex.Message));
                    }
                }

                // Initialise the events
                EventManager.Instance.Initialize(LoggingService);

                Application["GoToInstaller"] = "False";
            }
            else
            {
                Application["GoToInstaller"] = "True";
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Application["GoToInstaller"].ToString() == "True")
            {
                // Beford I redirect set it to false or we'll end up in a loop
                Application["GoToInstaller"] = "False";
                Response.Redirect("/install/");
            }
        }   

        protected void Application_EndRequest(object sender, EventArgs e)
        {
          
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            LoggingService.Error(Server.GetLastError());
        }

    }
}