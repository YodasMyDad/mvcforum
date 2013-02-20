using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Utilities;

namespace MVCForum.Website.Application
{
    public static class AppHelpers
    {
        #region Themes

        /// <summary>
        /// Gets the theme folders currently installed
        /// </summary>
        /// <returns></returns>
        public static List<string> GetThemeFolders()
        {
            var folders = new List<string>();
            var themeRootFolder = HttpContext.Current.Server.MapPath(string.Format("~/{0}", AppConstants.ThemeRootFolderName));
            if (Directory.Exists(themeRootFolder))
            {
                folders.AddRange(Directory.GetDirectories(themeRootFolder)
                                .Select(Path.GetFileName)
                                .Where(x => !x.ToLower().Contains("base")));
            }
            else 
            {
                throw new ApplicationException("Theme folder not found");
            }
            return folders;
        }

        #endregion

        #region Version Info

        /// <summary>
        /// Gets the main version number (Used by installer)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersionNo()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return string.Format("{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        /// Get the full version number shown in the admin
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersionNoFull()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// Get the previous version number if there is one from the web.config
        /// </summary>
        /// <returns></returns>
        public static string PreviousVersionNo()
        {
            return ConfigUtils.GetAppSetting("MVCForumVersion");
        }

        #endregion

        #region SEO

        private const string CanonicalNext = "<link href=\"{0}\" rel=\"next\" />";
        private const string CanonicalPrev = "<link href=\"{0}\" rel=\"prev\" />";
        private const string Canonical = "<link href=\"{0}\" rel=\"canonical\" />";

        public static string CanonicalPagingTag(int totalItemCount, int pageSize, HtmlHelper helper)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var currentAction = helper.ViewContext.RouteData.GetRequiredString("Action");
            var url = urlHelper.Action(currentAction, new {});

            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            var nextTag = string.Empty;
            var previousTag = string.Empty;

            var req = HttpContext.Current.Request["p"];
            var page = req != null ? Convert.ToInt32(req) : 1;

            // Sort the canonical tag out
            var canonicalTag = string.Format(Canonical, page <= 1 ? url : string.Format(AppConstants.PagingUrlFormat, url, page));

            // On the first page       
            if (pageCount > 1 & page <= 1)
            {
                nextTag = string.Format(CanonicalNext, string.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
            }

            // On a page greater than the first page, but not the last
            if (pageCount > 1 & page > 1 & page < pageCount)
            {
                nextTag = string.Format(CanonicalNext, string.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
                previousTag = string.Format(CanonicalPrev, string.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // On the last page
            if (pageCount > 1 & pageCount == page)
            {
                previousTag = string.Format(CanonicalPrev, string.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // return the canoncal tags
            return string.Concat(canonicalTag, Environment.NewLine, 
                                    nextTag, Environment.NewLine, 
                                    previousTag);
        }

        public static string CreatePageTitle(Entity entity, string fallBack)
        {
            if(entity != null)
            {
                if (entity is Category)
                {
                    var cat = entity as Category;
                    return cat.Name;
                }
                if (entity is Topic)
                {
                    var topic = entity as Topic;
                    return topic.Name;
                } 
            }
            return fallBack;
        }

        public static string CreateMetaDesc(Entity entity)
        {
            return "";
        }

        #endregion

        #region Urls

        public static string CategoryRssUrls(string slug)
        {
            return string.Format("/{0}/rss/{1}", AppConstants.CategoryUrlIdentifier, slug);
        }

        #endregion

        #region String
        
        public static string ConvertPostContent(string post)
        {
            // If using the BBCode Editor uncomment this line
            //post = StringUtils.ConvertBbCodeToHtml(post);

            // If using the PageDown/MarkDown Editor uncomment this line
            post = StringUtils.ConvertMarkDown(post);

            // Allow video embeds
            post = StringUtils.EmbedVideosInPosts(post);

            return post;
        }

        public static string ReturnBadgeUrl(string badgeFile)
        {
            return string.Concat("~/content/badges/", badgeFile);
        }

        #endregion

    }
}