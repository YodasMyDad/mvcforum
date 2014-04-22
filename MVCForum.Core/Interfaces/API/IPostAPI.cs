using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public partial interface IPostAPI
    {
        /// <summary>
        /// Create a post
        /// </summary>
        /// <param name="post"></param>
        Post Create(Post post);

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post"></param>
        void Delete(Post post);

        /// <summary>
        /// Get all the posts written by a member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<Post> GetPostsByMember(Guid memberId);

        /// <summary>
        /// Get all the posts belonging to a topic
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        IList<Post> GetPostsByTopicId(Guid topicId);

        /// <summary>
        /// Get all the posts that are solutions that have been written by the specified member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<Post> GetSolutionsWrittenByMember(Guid memberId);
    }
}
