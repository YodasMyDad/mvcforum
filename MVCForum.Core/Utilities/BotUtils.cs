using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.Utilities
{
    public static class BotUtils
    {
        public static bool UserIsBot()
        {
            if (HttpContext.Current.Request.UserAgent != null)
            {
                var userAgent = HttpContext.Current.Request.UserAgent.ToLower();
                var botKeywords = new List<string> { "bot", "spider", "google", "yahoo", "search", "crawl", "slurp", "msn", "teoma", "ask.com", "bing", "accoona" };
                return botKeywords.Any(userAgent.Contains);
            }
            return true;
        }
    }
}
