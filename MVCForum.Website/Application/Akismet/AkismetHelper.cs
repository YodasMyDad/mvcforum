namespace MvcForum.Web.Application.Akismet
{
    using System.Linq;
    using System.Web;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Utilities;

    public class AkismetHelper
    {
        private readonly ISettingsService _settingsService;

        public AkismetHelper(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        ///     Check whether a post is spam or not
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public bool IsSpam(Post post)
        {
            // If akisment is is not enable always return false
            if (_settingsService.GetSettings().EnableAkisment == null ||
                _settingsService.GetSettings().EnableAkisment == false)
            {
                return false;
            }

            // Akisment must be enabled
            var comment = new Comment
            {
                blog = StringUtils.CheckLinkHasHttp(_settingsService.GetSettings().ForumUrl),
                comment_type = "comment",
                comment_author = post.User.UserName,
                comment_author_email = post.User.Email,
                comment_content = post.PostContent,
                permalink = string.Empty,
                referrer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"],
                user_agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"],
                user_ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
            };
            var validator = new Validator(_settingsService.GetSettings().AkismentKey);
            return validator.IsSpam(comment);
        }

        /// <summary>
        ///     Check whether a topic is spam or not
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool IsSpam(Topic topic)
        {
            // If akisment is is not enable always return false
            if (_settingsService.GetSettings().EnableAkisment == null ||
                _settingsService.GetSettings().EnableAkisment == false)
            {
                return false;
            }

            // Akisment must be enabled
            var firstOrDefault = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
            if (firstOrDefault != null)
            {
                var comment = new Comment
                {
                    blog = StringUtils.CheckLinkHasHttp(_settingsService.GetSettings().ForumUrl),
                    comment_type = "comment",
                    comment_author = topic.User.UserName,
                    comment_author_email = topic.User.Email,
                    comment_content = firstOrDefault.PostContent,
                    permalink = string.Empty,
                    referrer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"],
                    user_agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"],
                    user_ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
                };
                var validator = new Validator(_settingsService.GetSettings().AkismentKey);
                return validator.IsSpam(comment);
            }
            return true;
        }
    }
}