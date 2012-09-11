using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public class PrivateMessageRepository : IPrivateMessageRepository
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
                                .OrderByDescending(x => x.DateSent)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, totalCount);
        }

        public PrivateMessage GetLastSentPrivateMessage(Guid Id)
        {
            return _context.PrivateMessage
                                .Where(x => x.UserFrom.Id == Id)
                                .OrderByDescending(x => x.DateSent)
                                .Take(1)
                                .SingleOrDefault();
        }

        public IList<PrivateMessage> GetAllSentByUser(Guid Id)
        {
            return _context.PrivateMessage
                                .Where(x => x.UserFrom.Id == Id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList();
        }

        public IList<PrivateMessage> GetAllReceivedByUser(Guid Id)
        {
            return _context.PrivateMessage
                                .Where(x => x.UserTo.Id == Id)
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

        public PrivateMessage Add(PrivateMessage item)
        {
            item.Id = GuidComb.GenerateComb();
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
