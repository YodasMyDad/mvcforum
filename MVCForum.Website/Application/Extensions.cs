using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;

namespace MVCForum.Website.Application
{
    public static class Extensions
    {
        /// <summary>
        /// Create an action link to an action in the Admin area.
        /// </summary>
        public static MvcHtmlString AdminActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string adminControllerName)
        {
            // http://stackoverflow.com/questions/2036305/how-to-specify-an-area-name-in-an-action-link
            return htmlHelper.ActionLink(linkText, actionName, adminControllerName, new { Area = "Admin" }, new { });
        }

        public static string MemberImage(this MembershipUser user,  int size)
        {
            return AppHelpers.MemberImage(user.Avatar, user.Email, user.Id, size);
        }

        /// <summary>
        /// Gets the site settings
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static Settings Settings(this HtmlHelper helper)
        {
            return DependencyResolver.Current.GetService<ISettingsService>().GetSettings();
        }

        public static string KiloFormat(this int num)
        {
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.#") + "M";

            if (num >= 10000)
                return (num / 1000D).ToString("#,0K");

            if (num >= 1000)
                return (num / 1000D).ToString("0.#") + "K";

            return num.ToString(CultureInfo.InvariantCulture);
        } 

        /// <summary>
        /// Gets the specific language text from the language key
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        //[Obsolete]
        public static string LanguageString(this HtmlHelper helper, string key)
        {
            var locService = ServiceFactory.Get<ILocalizationService>();
            return locService.GetResourceString(key);
        }

        public static string Lang(this HtmlHelper helper, string key)
        {
            var locService = ServiceFactory.Get<ILocalizationService>();
            return locService.GetResourceString(key);
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> list, Func<T, TKey> lookup) where TKey : struct
        {
            return list.Distinct(new StructEqualityComparer<T, TKey>(lookup));
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int currentPage, int pageSize, int totalItemCount, object routeValues, string actionOveride = null)
        {
            // how many pages to display in each page group const  	
            const int cGroupSize = SiteConstants.PagingGroupSize;
            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            if(pageCount <= 0)
            {
                return null;
            }

            // cleanup any out bounds page number passed  	
            currentPage = Math.Max(currentPage, 1);
            currentPage = Math.Min(currentPage, pageCount);

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var containerdiv = new TagBuilder("div");
            containerdiv.AddCssClass("pagination");
            var container = new TagBuilder("ul");
            var actionName = !string.IsNullOrEmpty(actionOveride) ? actionOveride : helper.ViewContext.RouteData.GetRequiredString("Action");

            // calculate the last page group number starting from the current page  	
            // until we hit the next whole divisible number  	
            var lastGroupNumber = currentPage;
            while ((lastGroupNumber % cGroupSize != 0)) lastGroupNumber++;

            // correct if we went over the number of pages  	
            var groupEnd = Math.Min(lastGroupNumber, pageCount);

            // work out the first page group number, we use the lastGroupNumber instead of  	
            // groupEnd so that we don't include numbers from the previous group if we went  	
            // over the page count  	
            var groupStart = lastGroupNumber - (cGroupSize - 1);

            // if we are past the first page  	
            if (currentPage > 1)
            {
                var previousli = new TagBuilder("li");
                var previous = new TagBuilder("a");
                previous.SetInnerText("<");
                previous.AddCssClass("previous");
                var routingValues = new RouteValueDictionary(routeValues) {{"p", currentPage - 1}};
                previous.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                previousli.InnerHtml = previous.ToString();
                container.InnerHtml += previousli;
            }

            // if we have past the first page group  	
            if (currentPage > cGroupSize)
            {
                var previousDotsli = new TagBuilder("li");
                var previousDots = new TagBuilder("a");
                previousDots.SetInnerText("...");
                previousDots.AddCssClass("previous-dots");
                var routingValues = new RouteValueDictionary(routeValues) {{"p", groupStart - cGroupSize}};
                previousDots.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                previousDotsli.InnerHtml = previousDots.ToString();
                container.InnerHtml += previousDotsli.ToString();
            }

            for (var i = groupStart; i <= groupEnd; i++)
            {
                var pageNumberli = new TagBuilder("li");
                pageNumberli.AddCssClass(((i == currentPage)) ? "active" : "p");
                var pageNumber = new TagBuilder("a");                
                pageNumber.SetInnerText((i).ToString());
                var routingValues = new RouteValueDictionary(routeValues) {{"p", i}};
                pageNumber.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                pageNumberli.InnerHtml = pageNumber.ToString();
                container.InnerHtml += pageNumberli.ToString();
            }

            // if there are still pages past the end of this page group  	
            if (pageCount > groupEnd)
            {
                var nextDotsli = new TagBuilder("li");
                var nextDots = new TagBuilder("a");
                nextDots.SetInnerText("...");
                nextDots.AddCssClass("next-dots");
                var routingValues = new RouteValueDictionary(routeValues) {{"p", groupEnd + 1}};
                nextDots.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                nextDotsli.InnerHtml = nextDots.ToString();
                container.InnerHtml += nextDotsli.ToString();
            }

            // if we still have pages left to show  	
            if (currentPage < pageCount)
            {
                var nextli = new TagBuilder("li");
                var next = new TagBuilder("a");
                next.SetInnerText(">");
                next.AddCssClass("next");
                var routingValues = new RouteValueDictionary(routeValues) {{"p", currentPage + 1}};
                next.MergeAttribute("href", urlHelper.Action(actionName, routingValues));
                nextli.InnerHtml = next.ToString();
                container.InnerHtml += nextli.ToString();
            }
            containerdiv.InnerHtml = container.ToString();
            return MvcHtmlString.Create(containerdiv.ToString());
        }
    }

    class StructEqualityComparer<T, TKey> : IEqualityComparer<T> where TKey : struct
    {

        Func<T, TKey> lookup;

        public StructEqualityComparer(Func<T, TKey> lookup)
        {
            this.lookup = lookup;
        }

        public bool Equals(T x, T y)
        {
            return lookup(x).Equals(lookup(y));
        }

        public int GetHashCode(T obj)
        {
            return lookup(obj).GetHashCode();
        }
    }
}