using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public partial interface ITopicAPI
    {
        Topic Get(Guid id);

        Topic GetTopicBySlug(string slug);

        /// <summary>
        /// Get all the topics that have been solved by a member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<Topic> GetSolvedTopicsByMember(Guid memberId);

        /// <summary>
        /// Get all topics in a category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        IList<Topic> GetTopicsByCatId(Guid categoryId);

        /// <summary>
        /// Get all topics written by a member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IList<Topic> GetTopicsByMember(Guid memberId);

        /// <summary>
        /// Get all topics, paged
        /// </summary>
        /// <returns></returns>
        PagedList<Topic> GetAllTopicsPaged(int pageIndex, int pageSize, int amountToTake);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="topic"></param>
        void Create(Topic topic);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="topic"></param>
        void Delete(Topic topic);
    }
}
