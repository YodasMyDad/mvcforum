using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
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

        public ILuceneService LuceneService
        {
            get { return DependencyResolver.Current.GetService<ILuceneService>(); }
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
            Application["Installing"] = "True";

            // Now check the version in the web.config
            var currentVersion = ConfigUtils.GetAppSetting("MVCForumVersion");

            // If the same carry on as normal
            LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            LoggingService.Error("START APP");

            // If the versions are different kick the installer into play
            if (currentVersion == Application["Version"].ToString())
            {
                Application["Installing"] = "False";

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

                // Don't go to installer
                Application[AppConstants.GoToInstaller] = "False";
            }
            else
            {
                // Go to the installer
                Application[AppConstants.GoToInstaller] = "True";
                
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Application[AppConstants.GoToInstaller].ToString() == "True")
            {
                // Beford I redirect set it to false or we'll end up in a loop
                // But set the Session to true as we'll check this in the base controller
                // of the normal app to stop people breaking out of the installer before its 
                // completed correctly
                Application[AppConstants.GoToInstaller] = "False";
                Response.Redirect("~/install/");
            }
        }


        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {

            //It's important to check whether session object is ready
            if (Application["Installing"].ToString() != "True")
            {
                if (HttpContext.Current.Session != null)
                {
                    var ci = (CultureInfo)this.Session["Culture"];
                    //Checking first if there is no value in session 
                    //and set default language 
                    //this can happen for first user's request
                    if (ci == null)
                    {
                        using (UnitOfWorkManager.NewUnitOfWork())
                        {
                            ci = new CultureInfo(SettingsService.GetSettings().DefaultLanguage.LanguageCulture);
                            this.Session["Culture"] = ci;   
                        }
                    }
                    //Finally setting culture for each request
                    Thread.CurrentThread.CurrentUICulture = ci;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
                }
            }

        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            //foreach (var item in HttpContext.Current.Items.Values)
            //{
            //    var disposableItem = item as IDisposable;

            //    if (disposableItem != null)
            //    {
            //        disposableItem.Dispose();
            //    }
            //}
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            LoggingService.Error(Server.GetLastError());
        }

    }
}