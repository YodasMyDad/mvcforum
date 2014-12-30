using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class VoteRepository : IVoteRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public VoteRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Vote Add(Vote item)
        {
            _context.Vote.Add(item);
            return item;
        }

        public Vote Get(Guid id)
        {
            return _context.Vote.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Vote item)
        {
            _context.Vote.Remove(item);
        }

        public void Update(Vote item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Vote.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;    
        }

        public IList<Vote> GetAllVotesByUser(Guid membershipId)
        {
            return _context.Vote.Where(x => x.VotedByMembershipUser.Id == membershipId).ToList();
        }

        public List<Vote> GetVotesByPosts(List<Guid> postIds)
        {
            return _context.Vote
                        .Include(x => x.User)
                        .Include(x => x.Post)
                        .Where(x => postIds.Contains(x.Post.Id)).ToList();
        }
    }
}
