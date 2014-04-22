using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

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

        public IPagedList<PrivateMessage> GetPagedSentMessagesByUser(int pageIndex, int pageSize, MembershipUser user)
        {
            var totalCount = _context.PrivateMessage.Count(x => x.UserFrom.Id == user.Id);

            // Get the topics using an efficient
            var results = _context.PrivateMessage
                                .Where(x => x.UserFrom.Id == user.Id)
                                .Where(x => x.IsSentMessage == true)
                                .OrderByDescending(x => x.DateSent)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, totalCount);
        }

        public IPagedList<PrivateMessage> GetPagedReceivedMessagesByUser(int pageIndex, int pageSize, MembershipUser user)
        {
            var totalCount = _context.PrivateMessage.Count(x => x.UserTo.Id == user.Id);

            // Get the topics using an efficient
            var results = _context.PrivateMessage
                                .Where(x => x.UserTo.Id == user.Id)
                                .Where(x => x.IsSentMessage != true)
                                .OrderByDescending(x => x.DateSent)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, totalCount);
        }

        public PrivateMessage GetLastSentPrivateMessage(Guid id)
        {
            return _context.PrivateMessage.FirstOrDefault(x => x.UserFrom.Id == id);
        }

        public PrivateMessage GetMatchingSentPrivateMessage(string title, DateTime date, Guid senderId, Guid receiverId)
        {
            return _context.PrivateMessage
                .FirstOrDefault(x => x.Subject == title && x.DateSent == date && x.UserFrom.Id == senderId && x.UserTo.Id == receiverId && x.IsSentMessage == true);
        }

        public IList<PrivateMessage> GetAllSentByUser(Guid id)
        {
            return _context.PrivateMessage
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
                                .Where(x => x.UserFrom.Id == senderId && x.UserTo.Id == receiverId)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
        }

        public int NewPrivateMessageCount(Guid userId)
        {
            return _context.PrivateMessage.Count(x => x.UserTo.Id == userId && !x.IsRead && x.IsSentMessage != true);
        }

        public PrivateMessage Add(PrivateMessage item)
        {
            return _context.PrivateMessage.Add(item);
        }

        public PrivateMessage Get(Guid id)
        {
            return _context.PrivateMessage.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(PrivateMessage item)
        {
            _context.PrivateMessage.Remove(item);
        }

        public void Update(PrivateMessage item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.PrivateMessage.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified; 
        }
    }
}
