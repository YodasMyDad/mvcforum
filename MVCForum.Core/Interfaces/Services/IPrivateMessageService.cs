namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.General;

    public partial interface IPrivateMessageService : IContextService
    {
        PrivateMessage SanitizeMessage(PrivateMessage privateMessage);
        PrivateMessage Add(PrivateMessage message);
        PrivateMessage Get(Guid id);
        Task<PaginatedList<PrivateMessageListItem>> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser user);

        Task<PaginatedList<PrivateMessage>> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser toUser,
            MembershipUser fromUser);

        PrivateMessage GetLastSentPrivateMessage(Guid id);
        PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId);
        IList<PrivateMessage> GetAllSentByUser(Guid id);

        /// <summary>
        ///     Get a list of private messages older than X days
        /// </summary>
        /// <param name="days">Amount of days to go back</param>
        /// <returns></returns>
        IList<PrivateMessage> GetPrivateMessagesOlderThan(int days);

        int NewPrivateMessageCount(Guid userId);
        IList<PrivateMessage> GetAllReceivedByUser(Guid id);
        IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId);
        void DeleteMessage(PrivateMessage message);
    }
}