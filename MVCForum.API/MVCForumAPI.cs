using MVCForum.Domain.Interfaces.API;

namespace MVCForum.API
{
    public class MVCForumAPI : IMVCForumAPI
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MVCForumAPI(IMemberAPI memberAPI, IPostAPI postAPI, ITopicAPI topicAPI, ICategoryAPI categoryAPI)
        {
            Member = memberAPI;
            Post = postAPI;
            Topic = topicAPI;
            Category = categoryAPI;
        }

        public IMemberAPI Member { get; set; }
        public IPostAPI Post { get; set; }
        public ITopicAPI Topic { get; set; }
        public ICategoryAPI Category { get; set; }
    }
}
