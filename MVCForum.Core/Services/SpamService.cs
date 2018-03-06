namespace MvcForum.Core.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using ExternalServices;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Spam;
    using Utilities;

    public partial class SpamService : ISpamService
    {
        private Settings _settings;
        private readonly ISettingsService _settingsService;

        public SpamService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settings = settingsService.GetSettings();
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _settingsService.RefreshContext(context);
            _settings = _settingsService.GetSettings();
        }

        /// <inheritdoc />
        public Task<int> SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSpam(Post post)
        {
            // If akisment is is not enable always return false
            if (_settings.EnableAkisment == null ||
                _settings.EnableAkisment == false)
            {
                return false;
            }

            return BaseCheck(post.User.UserName, post.User.Email, post.PostContent);
        }

        /// <inheritdoc />
        public bool IsSpam(Topic topic)
        {
            // If akisment is is not enable always return false
            if (_settings.EnableAkisment == null ||
                _settings.EnableAkisment == false)
            {
                return false;
            }

            // Akisment must be enabled
            // We may get a new topic, so it won't have any posts
            if (topic.Posts != null && topic.Posts.Any())
            {
                var firstOrDefault = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
                if (firstOrDefault != null)
                {
                    return BaseCheck(topic.User.UserName, topic.User.Email, firstOrDefault.PostContent);
                }
            }
            else
            {
                return BaseCheck(topic.User.UserName, topic.User.Email, topic.Name);
            }
            return false;
        }

        /// <inheritdoc />
        public bool IsSpam(string content)
        {
            // If akisment is is not enable always return false
            if (_settings.EnableAkisment == null ||
                _settings.EnableAkisment == false)
            {
                return false;
            }

            return BaseCheck(string.Empty, string.Empty, content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool BaseCheck(string username, string email, string content)
        {
            var comment = new Comment
            {
                blog = StringUtils.CheckLinkHasHttp(_settings.ForumUrl),
                comment_type = "comment",
                permalink = string.Empty,
                referrer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"],
                user_agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"],
                user_ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                comment.comment_author = username;
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                comment.comment_author_email = email;
            }
            if (!string.IsNullOrWhiteSpace(content))
            {
                comment.comment_content = content;
            }

            var validator = new Validator(_settings.AkismentKey);
            return validator.IsSpam(comment);
        }
    }
}