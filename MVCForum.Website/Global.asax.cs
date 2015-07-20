using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataAnnotationsExtensions.ClientValidation;
using LowercaseRoutesMVC;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.IOC;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.ScheduledJobs;

namespace MVCForum.Website
{
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

        public ILocalizationService LocalizationService
        {
            get { return DependencyResolver.Current.GetService<ILocalizationService>(); }
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

            // Start unity
            var unityContainer = UnityHelper.Start();
            
            // Run scheduled tasks
            ScheduledRunner.Run(unityContainer);

            // Register Data annotations
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();  

            // Store the value for use in the app
            Application["Version"] = AppHelpers.GetCurrentVersionNo();

            // If the same carry on as normal
            LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            LoggingService.Error("START APP");

            // Set default theme
            var defaultTheme = "Metro";

            // Only load these IF the versions are the same
            if (AppHelpers.SameVersionNumbers())
            {
                // Get the theme from the database.
                defaultTheme = SettingsService.GetSettings().Theme;

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

            }

            // Set the view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ForumViewEngine(defaultTheme));

            // Initialise the events
            EventManager.Instance.Initialize(LoggingService);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!AppHelpers.IsStaticResource(this.Request) && !AppHelpers.SameVersionNumbers() && !AppHelpers.InInstaller())
            {
                Response.Redirect(string.Concat("~", AppConstants.InstallerUrl));
            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {

            //It's important to check whether session object is ready
            if (!AppHelpers.InInstaller())
            {
                if (HttpContext.Current.Session != null)
                {
                    // Set the culture per request
                    var ci = new CultureInfo(LocalizationService.CurrentLanguage.LanguageCulture);
                    Thread.CurrentThread.CurrentUICulture = ci;
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
                }
            }

        }

        //protected void Application_EndRequest(object sender, EventArgs e)
        //{

        //}

        protected void Application_Error(object sender, EventArgs e)
        {
            LoggingService.Error(Server.GetLastError());
        }

    }
}