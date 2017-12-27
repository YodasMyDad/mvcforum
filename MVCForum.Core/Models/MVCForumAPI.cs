using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Domain.DomainModel
{
    public class MVCForumAPI : IMVCForumAPI
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topicService"></param>
        /// <param name="postService"></param>
        /// <param name="categoryService"></param>
        public MVCForumAPI(ITopicService topicService, IPostService postService, ICategoryService categoryService)
        {
            TopicService = topicService;
            PostService = postService;
            CategoryService = categoryService;
        }

        public ITopicService TopicService { get; set; }

        public IPostService PostService { get; set; }

        public ICategoryService CategoryService { get; set; }
    }
}
