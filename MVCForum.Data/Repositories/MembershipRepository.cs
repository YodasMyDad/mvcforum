using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class MembershipRepository : IMembershipRepository
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
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.UserName.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Returns a user by their facebook id
        /// </summary>
        /// <param name="facebookId"></param>
        /// <returns></returns>
        public MembershipUser GetUserByFacebookId(long facebookId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.FacebookId == facebookId);
        }

        public MembershipUser GetUserByTwitterId(string twitterId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.TwitterAccessToken == twitterId);
        }

        public MembershipUser GetUserByGoogleId(string googleId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.GoogleAccessToken == googleId);
        }

        public MembershipUser GetUserByOpenIdToken(string openId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.MiscAccessToken == openId);
        }

        public IList<MembershipUser> SearchMembers(string username, int amount)
        {
            return _context.MembershipUser
                            .Where(x => x.UserName.ToUpper().Contains(username.ToUpper()))
                            .OrderBy(x => x.UserName)
                            .Take(amount)
                            .ToList();
        }

        public IList<MembershipUser> GetActiveMembers()
        {
            // Get members that last activity date is valid
            var date = DateTime.UtcNow.AddMinutes(-AppConstants.TimeSpanInMinutesToShowMembers);
            return _context.MembershipUser
                .Where(x => x.LastActivityDate > date)
                .ToList();
        }

        public IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts)
        {
            var registerEnd = DateTime.UtcNow;
            var registerStart = registerEnd.AddDays(-amoutOfDaysSinceRegistered);
            return _context.MembershipUser
                .Where(x =>
                        x.Posts.Count <= amoutOfPosts &&
                        x.CreateDate > registerStart && 
                        x.CreateDate <= registerEnd)
                .ToList();
        }

        public MembershipUser GetUserBySlug(string slug)
        {

            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.Slug == slug);

        }

        public MembershipUser GetUserByEmail(string email)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.Email == email);
        }

        public IList<MembershipUser> GetUserBySlugLike(string slug)
        {
            return _context.MembershipUser
                    .Include(x => x.Roles)
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
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(x => x.Id == id);
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
            var totalCount = _context.MembershipUser.Count();
            var results = _context.MembershipUser
                                .OrderBy(x => x.UserName)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, totalCount);
        }

        public PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize)
        {
            var query = _context.MembershipUser
                .Where(x => x.UserName.ToUpper().Contains(search.ToUpper()) || x.Email.ToUpper().Contains(search.ToUpper()));

            var results = query
                .OrderBy(x => x.UserName)
                .Skip((pageIndex - 1)*pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, query.Count());
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <param name="newUser"></param>
        public MembershipUser Add(MembershipUser newUser)
        {
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
