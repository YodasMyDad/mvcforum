using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPrivateMessageRepository
    {
        IPagedList<PrivateMessage> GetPagedSentMessagesByUser(int pageIndex, int pageSize, MembershipUser user);
        IPagedList<PrivateMessage> GetPagedReceivedMessagesByUser(int pageIndex, int pageSize, MembershipUser user);
        PrivateMessage GetLastSentPrivateMessage(Guid id);
        PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId);
        IList<PrivateMessage> GetAllSentByUser(Guid id);
        IList<PrivateMessage> GetAllReceivedByUser(Guid id);
        IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId);
        int NewPrivateMessageCount(Guid userId);
        PrivateMessage Add(PrivateMessage item);
        PrivateMessage Get(Guid id);
        void Delete(PrivateMessage item);
        void Update(PrivateMessage item);
    }
}
