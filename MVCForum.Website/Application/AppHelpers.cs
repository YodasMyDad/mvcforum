namespace MvcForum.Web.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Models.Entities;
    using Core.Providers.Storage;
    using Core.Utilities;

    public static class AppHelpers
    {
        #region Application

        /// <summary>
        ///     Returns true if the requested resource is one of the typical resources that needn't be processed by the cms engine.
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <returns>True if the request targets a static resource file.</returns>
        /// <remarks>
        ///     These are the file extensions considered to be static resources:
        ///     .css
        ///     .gif
        ///     .png
        ///     .jpg
        ///     .jpeg
        ///     .js
        ///     .axd
        ///     .ashx
        /// </remarks>
        public static bool IsStaticResource(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var path = request.Path;
            var extension = VirtualPathUtility.GetExtension(path);

            if (extension == null)
            {
                return false;
            }

            switch (extension.ToLower())
            {
                case ".axd":
                case ".ashx":
                case ".bmp":
                case ".css":
                case ".gif":
                case ".htm":
                case ".html":
                case ".ico":
                case ".jpeg":
                case ".jpg":
                case ".js":
                case ".png":
                case ".rar":
                case ".zip":
                    return true;
            }

            return false;
        }

        #endregion

        #region Themes

        /// <summary>
        ///     Gets the theme folders currently installed
        /// </summary>
        /// <returns></returns>
        public static List<string> GetThemeFolders()
        {
            var folders = new List<string>();
            var themeRootFolder = HostingEnvironment.MapPath($"~/{ForumConfiguration.Instance.ThemeRootFolderName}");
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

        #region SEO

        private const string CanonicalNext = "<link href=\"{0}\" rel=\"next\" />";
        private const string CanonicalPrev = "<link href=\"{0}\" rel=\"prev\" />";
        private const string Canonical = "<link href=\"{0}\" rel=\"canonical\" />";

        public static string CanonicalPagingTag(int totalItemCount, int pageSize, HtmlHelper helper)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var currentAction = helper.ViewContext.RouteData.GetRequiredString("Action");
            var url = urlHelper.Action(currentAction, new { });

            var pageCount = (int) Math.Ceiling(totalItemCount / (double) pageSize);

            var nextTag = string.Empty;
            var previousTag = string.Empty;

            var req = HttpContext.Current.Request["p"];
            var page = req != null ? Convert.ToInt32(req) : 1;

            // Sort the canonical tag out
            var canonicalTag = string.Format(Canonical,
                page <= 1 ? url : string.Format(Constants.PagingUrlFormat, url, page));

            // On the first page       
            if ((pageCount > 1) & (page <= 1))
            {
                nextTag = string.Format(CanonicalNext, string.Format(Constants.PagingUrlFormat, url, page + 1));
            }

            // On a page greater than the first page, but not the last
            if ((pageCount > 1) & (page > 1) & (page < pageCount))
            {
                nextTag = string.Format(CanonicalNext, string.Format(Constants.PagingUrlFormat, url, page + 1));
                previousTag = string.Format(CanonicalPrev, string.Format(Constants.PagingUrlFormat, url, page - 1));
            }

            // On the last page
            if ((pageCount > 1) & (pageCount == page))
            {
                previousTag = string.Format(CanonicalPrev, string.Format(Constants.PagingUrlFormat, url, page - 1));
            }

            // return the canoncal tags
            return string.Concat(canonicalTag, Environment.NewLine,
                nextTag, Environment.NewLine,
                previousTag);
        }

        public static string CreatePageTitle(IBaseEntity entity, string fallBack)
        {
            if (entity != null)
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

        public static string CreateMetaDesc(IBaseEntity entity)
        {
            return "";
        }

        #endregion

        #region Urls

        public static bool Ping(string url)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.Timeout = 3000;
                request.AllowAutoRedirect = false; // find out if this site is up and don't follow a redirector
                request.Method = "HEAD";

                using (var response = request.GetResponse())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string CategoryRssUrls(string slug)
        {
            return $"/{ForumConfiguration.Instance.CategoryUrlIdentifier}/rss/{slug}";
        }

        #endregion

        #region String

        public static string ReturnBadgeUrl(string badgeFile)
        {
            return string.Concat("~/content/badges/", badgeFile);
        }

        #endregion

        #region Files

        public static bool FileIsImage(string file)
        {
            var imageFileTypes = new List<string>
            {
                ".jpg",
                ".jpeg",
                ".gif",
                ".bmp",
                ".png"
            };
            return imageFileTypes.Any(file.Contains);
        }

        public static string MemberImage(string avatar, string email, Guid userId, int size)
        {
            if (!string.IsNullOrWhiteSpace(avatar))
            {
                // Has an avatar image
                var storageProvider = StorageProvider.Current;
                return storageProvider.BuildFileUrl(userId, "/", avatar,
                    string.Format("?width={0}&crop=0,0,{0},{0}", size));
            }

            return StringUtils.GetGravatarImage(email, size);
        }

        public static string CategoryImage(string image, Guid categoryId, int size)
        {
            var sizeFormat = string.Format("?width={0}&crop=0,0,{0},{0}", size);
            if (!string.IsNullOrWhiteSpace(image))
            {
                var storageProvider = StorageProvider.Current;
                return storageProvider.BuildFileUrl(categoryId, "/", image, sizeFormat);
            }
            //TODO - Return default image for category
            return null;
        }

        #endregion
    }
}