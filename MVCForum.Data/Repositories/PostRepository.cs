using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;


namespace MVCForum.Data.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly MVCForumContext _context;
        public PostRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IList<Post> GetAll()
        {
            return _context.Post.ToList();
        }

        public IList<Post> GetLowestVotedPost(int amountToTake)
        {
            return _context.Post
                .Where(x => x.VoteCount < 0)
                .OrderByDescending(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();
        }

        public IList<Post> GetHighestVotedPost(int amountToTake)
        {
            return _context.Post                
                .Where(x => x.VoteCount > 0)
                .OrderByDescending(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();
        }

        public IList<Post> GetByMember(Guid memberId, int amountToTake)
        {
            return _context.Post
                .Where(x => x.User.Id == memberId)
                .OrderBy(x => x.DateCreated)
                .Take(amountToTake)
                .ToList();
        }

        /// <summary>
        /// Get all posts that are solutions, by user
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Post> GetSolutionsByMember(Guid memberId)
        {
            return _context.Post
                .Where(x => x.User.Id == memberId)
                .Where(x => x.IsSolution)
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }

        public IList<Post> GetPostsByTopic(Guid topicId)
        {
            return _context.Post
                .Where(x => x.Topic.Id == topicId)
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }

        public IList<Post> GetPostsByMember(Guid memberId)
        {
            return _context.Post
                .Where(x => x.User.Id == memberId)
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }

        public IList<Post> GetAllSolutionPosts()
        {
            return _context.Post
                .Where(x => x.IsSolution)
                .OrderByDescending(x => x.DateCreated)
                .ToList();
        }

        public Post Add(Post post)
        {
            post.Id = Guid.NewGuid();
            return _context.Post.Add(post);
        }

        public Post Get(Guid id)
        {
            return _context.Post.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Post item)
        {
            _context.Post.Remove(item);
        }

        public void Update(Post item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Post.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }

        public int PostCount()
        {
            return GetAll().Count();
        }
    }
}
