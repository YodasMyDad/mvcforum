using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPrivateMessageRepository
    {
        IPagedList<PrivateMessageListItem> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser user);
        IPagedList<PrivateMessage> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser toUser, MembershipUser fromUser);
        PrivateMessage GetLastSentPrivateMessage(Guid id);
        PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId);
        IList<PrivateMessage> GetAllSentByUser(Guid id);
        IList<PrivateMessage> GetAllReceivedByUser(Guid id);
        IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId);
        int NewPrivateMessageCount(Guid userId);
        PrivateMessage Add(PrivateMessage item);
        PrivateMessage Get(Guid id);
        void Delete(PrivateMessage item);
        /// <summary>
        /// Get a list of private messages older than X days
        /// </summary>
        /// <param name="days">Amount of days to go back</param>
        /// <returns></returns>
        IList<PrivateMessage> GetPrivateMessagesOlderThan(int days);
    }
}
