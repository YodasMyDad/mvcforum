namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using Domain.DomainModel;
    using Domain.Events;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;
    using Utilities;

    public partial class PrivateMessageService : IPrivateMessageService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="cacheService"></param>
        public PrivateMessageService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public PrivateMessage SanitizeMessage(PrivateMessage privateMessage)
        {
            privateMessage.Message = StringUtils.GetSafeHtml(privateMessage.Message);
            return privateMessage;
        }

        /// <summary>
        /// Add a private message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public PrivateMessage Add(PrivateMessage message)
        {
            // This is the message that the other user sees
            message = SanitizeMessage(message);
            message.DateSent = DateTime.UtcNow;
            message.IsSentMessage = false;

            var e = new PrivateMessageEventArgs { PrivateMessage = message };
            EventManager.Instance.FireBeforePrivateMessage(this, e);

            if (!e.Cancel)
            {
                message = _context.PrivateMessage.Add(message);

                // We create a sent message that sits in the users sent folder, this is 
                // so that if the receiver deletes the message - The sender still has a record of it.
                var sentMessage = new PrivateMessage
                {
                    IsSentMessage = true,
                    DateSent = message.DateSent,
                    Message = message.Message,
                    UserFrom = message.UserFrom,
                    UserTo = message.UserTo
                };

                _context.PrivateMessage.Add(sentMessage);

                EventManager.Instance.FireAfterPrivateMessage(this, new PrivateMessageEventArgs { PrivateMessage = message });
            }

            // Return the main message
            return message;
        }

        /// <summary>
        /// Return a private message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                                                        .Include(x => x.UserTo)
                                                                        .Include(x => x.UserFrom)
                                                                        .FirstOrDefault(x => x.Id == id));
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

        public IPagedList<PrivateMessage> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser toUser, MembershipUser fromUser)
        {
            var query = _context.PrivateMessage
               .AsNoTracking()
               .Include(x => x.UserFrom)
               .Include(x => x.UserTo)
               .Where(x => (x.UserFrom.Id == fromUser.Id && x.UserTo.Id == toUser.Id && x.IsSentMessage != true) || (x.UserFrom.Id == toUser.Id && x.UserTo.Id == fromUser.Id && x.IsSentMessage == true))
               .OrderByDescending(x => x.DateSent);

            var total = query.Count();

            var results = query
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<PrivateMessage>(results, pageIndex, pageSize, total);
        }


        /// <summary>
        /// Gets the last sent private message from a specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage GetLastSentPrivateMessage(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetLastSentPrivateMessage-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                                                        .AsNoTracking()
                                                                        .Include(x => x.UserTo)
                                                                        .Include(x => x.UserFrom)
                                                                        .FirstOrDefault(x => x.UserFrom.Id == id));
        }

        public PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetMatchingSentPrivateMessage-", date.ToString("d"), "-", senderId, "-", receiverId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .FirstOrDefault(x => x.DateSent == date && x.UserFrom.Id == senderId && x.UserTo.Id == receiverId && x.IsSentMessage == true));
        }

        /// <summary>
        /// Gets all private messages sent by a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllSentByUser(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetAllSentByUser-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                .AsNoTracking()
                                .Include(x => x.UserTo)
                                .Include(x => x.UserFrom)
                                .Where(x => x.UserFrom.Id == id)
                                .OrderByDescending(x => x.DateSent)
                                .ToList());
        }

        public IList<PrivateMessage> GetPrivateMessagesOlderThan(int days)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetPrivateMessagesOlderThan-", days);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var date = DateTime.UtcNow.AddDays(-days);
                return _context.PrivateMessage.Where(x => x.DateSent <= date).ToList();
            });
        }

        /// <summary>
        /// Returns a count of any new messages the user has
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int NewPrivateMessageCount(Guid userId)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "NewPrivateMessageCount-", userId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                                                        .AsNoTracking()
                                                                        .Include(x => x.UserTo)
                                                                        .Include(x => x.UserFrom)
                                                                        .Where(x => x.UserTo.Id == userId && !x.IsRead && x.IsSentMessage != true)
                                                                        .GroupBy(x => x.UserFrom.Id).Count());
        }

        /// <summary>
        /// Gets all private messages received by a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllReceivedByUser(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetAllReceivedByUser-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                                                        .AsNoTracking()
                                                                        .Where(x => x.UserTo.Id == id)
                                                                        .OrderByDescending(x => x.DateSent)
                                                                        .ToList());
        }


        /// <summary>
        /// get all private messages sent from one user to another
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId)
        {
            var cacheKey = string.Concat(CacheKeys.PrivateMessage.StartsWith, "GetAllByUserToAnotherUser-", senderId, receiverId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PrivateMessage
                                                                    .AsNoTracking()
                                                                    .Include(x => x.UserTo)
                                                                    .Include(x => x.UserFrom)
                                                                    .Where(x => x.UserFrom.Id == senderId && x.UserTo.Id == receiverId)
                                                                    .OrderByDescending(x => x.DateSent)
                                                                    .ToList());
        }

        /// <summary>
        /// Delete a private message
        /// </summary>
        /// <param name="message"></param>
        public void DeleteMessage(PrivateMessage message)
        {
            _context.PrivateMessage.Remove(message);
        }

    }
}
