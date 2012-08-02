using System;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.DomainModel;

namespace MVCForum.API
{
    public class PostAPI : IPostAPI
    {
        private readonly IPostRepository _postRepository;

        public PostAPI (IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public Post Create(Post post)
        {
            post.IsSolution = false;
            post.DateCreated = DateTime.Now;
            post.DateEdited = DateTime.Now;
            _postRepository.Add(post);
            return post;
        }

        public IList<Post> GetSolutionsWrittenByMember(Guid memberId)
        {
            return _postRepository.GetSolutionsByMember(memberId);
        }

        public IList<Post> GetPostsByTopicId(Guid topicId)
        {
            return _postRepository.GetPostsByTopic(topicId);
        }

        public IList<Post> GetPostsByMember(Guid memberId)
        {
            return _postRepository.GetPostsByMember(memberId);
        }

        public void Delete(Post post)
        {
            _postRepository.Delete(post);
        }
    }
}
