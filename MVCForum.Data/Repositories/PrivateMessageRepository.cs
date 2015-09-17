using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public partial class PrivateMessageRepository : IPrivateMessageRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public PrivateMessageRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IPagedList<PrivateMessageListItem> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser user)
        {
            var query = _context.PrivateMessage
                .AsNoTracking()
                .Include(x => x.UserFrom)
                .Include(x => x.UserTo)
                .Where(x => (x.UserTo.Id == user.Id && x.IsSentMessage != true) || (x.UserFrom.Id == user.Id && x.IsSentMessage == true))
                .Select(x => new PrivateMessageListItem
                {
                    Date = x.DateSent,
                    User = (x.IsSentMessage == true ? x.UserTo : x.UserFrom),
                    HasUnreadMessages = (x.IsSentMessage != true && x.UserFrom.Id != user.Id && (x.IsRead == false))
                })
                .GroupBy(x => x.User.Id)
                .Select(x => x.OrderByDescending(d => d.Date).FirstOrDefault())
                .OrderByDescending(x => x.Date);

            var total = query.Count();

            var results = query
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<PrivateMessageListItem>(results, pageIndex, pageSize, total);
        }

        public IPagedList<PrivateMessage> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser user, MembershipUser fromUser)
        {
            var query = _context.PrivateMessage
                .AsNoTracking()
                .Include(x => x.UserFrom)
                .Include(x => x.UserTo)
                .Where(x => (x.UserFrom.Id == fromUser.Id && x.UserTo.Id == user.Id && x.IsSentMessage != true) || (x.UserFrom.Id == user.Id && x.UserTo.Id == fromUser.Id && x.IsSentMessage == true))
                .OrderByDescending(x => x.DateSent);

            var total = query.Count();

            var results = query
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, total);
        }

        public PrivateMessage GetLastSentPrivateMessage(Guid id)
        {
            return _context.PrivateMessage
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .FirstOrDefault(x => x.UserFrom.Id == id);
        }

        public PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId)
        {
            return _context.PrivateMessage
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .FirstOrDefault(x => x.DateSent == date && x.UserFrom.Id == senderId && x.UserTo.Id == receiverId && x.IsSentMessage == true);
        }

        public IList<PrivateMessage> GetAllSentByUser(Guid id)
        {
            return _context.PrivateMessage
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .Where(x => x.UserFrom.Id == id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
        }

        public IList<PrivateMessage> GetAllReceivedByUser(Guid id)
        {
            return _context.PrivateMessage
                                .Where(x => x.UserTo.Id == id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
        }

        public IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId)
        {
            return _context.PrivateMessage
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .Where(x => x.UserFrom.Id == senderId && x.UserTo.Id == receiverId)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
        }

        public int NewPrivateMessageCount(Guid userId)
        {
            return _context.PrivateMessage
                            .Include(x => x.UserTo)
                            .Include(x => x.UserFrom)
                            .Where(x => x.UserTo.Id == userId && !x.IsRead && x.IsSentMessage != true)
                            .GroupBy(x => x.UserFrom.Id).Count();
        }

        public PrivateMessage Add(PrivateMessage item)
        {
            return _context.PrivateMessage.Add(item);
        }

        public PrivateMessage Get(Guid id)
        {
            return _context.PrivateMessage
                            .Include(x => x.UserTo)
                            .Include(x => x.UserFrom)
                            .FirstOrDefault(x => x.Id == id);
        }

        public void Delete(PrivateMessage item)
        {
            _context.PrivateMessage.Remove(item);
        }

        public IList<PrivateMessage> GetPrivateMessagesOlderThan(int days)
        {
            var date = DateTime.UtcNow.AddDays(-days);
            return _context.PrivateMessage.Where(x => x.DateSent <= date).ToList();
        }
    }
}
