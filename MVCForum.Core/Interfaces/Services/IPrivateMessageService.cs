using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IPrivateMessageService
    {
        PrivateMessage SanitizeMessage(PrivateMessage privateMessage);
        PrivateMessage Add(PrivateMessage message);
        PrivateMessage Get(Guid id);
        void Save(PrivateMessage id);
        IPagedList<PrivateMessage> GetPagedSentMessagesByUser(int pageIndex, int pageSize, MembershipUser user);
        IPagedList<PrivateMessage> GetPagedReceivedMessagesByUser(int pageIndex, int pageSize, MembershipUser user);
        PrivateMessage GetLastSentPrivateMessage(Guid Id);
        IList<PrivateMessage> GetAllSentByUser(Guid Id);
        int NewPrivateMessageCount(Guid userId);
        IList<PrivateMessage> GetAllReceivedByUser(Guid Id);
        IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId);
        void DeleteMessage(PrivateMessage message);
    }
}
