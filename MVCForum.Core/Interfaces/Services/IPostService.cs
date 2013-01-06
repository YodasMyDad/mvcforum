using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IPostService
    {
        Post SanitizePost(Post post);
        IEnumerable<Post> GetAll();
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId);
        Post Add(Post post);
        Post Get(Guid postId);
        void SaveOrUpdate(Post post);
        bool Delete(Post post);
        IList<Post> GetSolutionsByMember(Guid memberId);
        int PostCount();
        Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions);
    }
}
