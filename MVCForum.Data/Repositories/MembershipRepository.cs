using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public class MembershipRepository : IMembershipRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public MembershipRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get a user by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public MembershipUser GetUser(string username)
        {
            return _context.MembershipUser.SingleOrDefault(name => name.UserName.ToUpper().Contains(username.ToUpper()));
        }

        public IList<MembershipUser> SearchMembers(string username, int amount)
        {
            return _context.MembershipUser
                            .Where(x => x.UserName.ToUpper().Contains(username.ToUpper()))
                            .OrderBy(x => x.UserName)
                            .Take(amount)
                            .ToList();
        }

        public MembershipUser GetUserBySlug(string slug)
        {

            return _context.MembershipUser.SingleOrDefault(name => name.Slug == slug);

        }

        public MembershipUser GetUserByEmail(string email)
        {
            return _context.MembershipUser.SingleOrDefault(name => name.Email == email);
        }

        public IList<MembershipUser> GetUserBySlugLike(string slug)
        {
            return _context.MembershipUser
                    .Where(name => name.Slug.ToUpper().Contains(slug.ToUpper()))
                    .ToList();
        }

        public IList<MembershipUser> GetUsersById(List<Guid> guids)
        {
            return _context.MembershipUser
              .Where(x => guids.Contains(x.Id))
              .ToList();
        }

        public MembershipUser Get(Guid id)
        {
            return _context.MembershipUser.FirstOrDefault(x => x.Id == id);
        }

        public IList<MembershipUser> GetLatestUsers(int amountToTake)
        {
            return _context.MembershipUser
              .OrderByDescending(x => x.CreateDate)
              .Take(amountToTake)
              .ToList();
        }

        public IList<MembershipUser> GetLowestPointUsers(int amountToTake)
        {
            return _context.MembershipUser
                 .Join(_context.MembershipUserPoints, // The sequence to join to the first sequence.
                        user => user.Id, // A function to extract the join key from each element of the first sequence.
                        userPoints => userPoints.User.Id, // A function to extract the join key from each element of the second sequence
                        (user, userPoints) => new { MembershipUser = user, UserPoints = userPoints } // A function to create a result element from two matching elements.
                    )
                .OrderBy(x => x.UserPoints)
                .Take(amountToTake)
                .Select(t => t.MembershipUser)
                .ToList();
        }

        public int MemberCount()
        {
            return _context.MembershipUser.Count();
        }

        public PagedList<MembershipUser> GetAll(int pageIndex, int pageSize)
        {
            var results = _context.MembershipUser
                                .OrderBy(x => x.UserName)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, results.Count);
        }

        public PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize)
        {
            var results = _context.MembershipUser
                .Where(x => x.UserName.ToUpper().Contains(search.ToUpper()))
                .OrderBy(x => x.UserName)
                .Skip((pageIndex - 1)*pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, results.Count);
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <param name="newUser"></param>
        public MembershipUser Add(MembershipUser newUser)
        {
            newUser.Id = GuidComb.GenerateComb();
            return _context.MembershipUser.Add(newUser);
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <returns></returns>
        public IList<MembershipUser> GetAll()
        {
            return _context.MembershipUser.ToList();
        }

        /// <summary>
        /// Generic single entity delete
        /// </summary>
        /// <param name="user"></param>
        public void Delete(MembershipUser user)
        {
            _context.MembershipUser.Remove(user);
        }

        public void Update(MembershipUser item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.MembershipUser.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified; 
        }
    }
}
