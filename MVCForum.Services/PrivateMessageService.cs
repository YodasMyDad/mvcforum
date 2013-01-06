using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public class PrivateMessageService : IPrivateMessageService
    {
        private readonly IPrivateMessageRepository _privateMessageRepository;
        private readonly IMembershipRepository _membershipRepository;

        public PrivateMessageService(IPrivateMessageRepository privateMessageRepository, IMembershipRepository membershipRepository)
        {
            _privateMessageRepository = privateMessageRepository;
            _membershipRepository = membershipRepository;
        }

        public PrivateMessage SanitizeMessage(PrivateMessage privateMessage)
        {
            privateMessage.Message = StringUtils.GetSafeHtml(privateMessage.Message);
            privateMessage.Subject = StringUtils.SafePlainText(privateMessage.Subject);
            return privateMessage;
        }

        /// <summary>
        /// Add a private message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public PrivateMessage Add(PrivateMessage message)
        {
            message = SanitizeMessage(message);
            message.DateSent = DateTime.Now;
            return _privateMessageRepository.Add(message);
        }

        /// <summary>
        /// Return a private message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage Get(Guid id)
        {
            return _privateMessageRepository.Get(id);
        }

        /// <summary>
        /// Save a private message
        /// </summary>
        /// <param name="message"></param>
        public void Save(PrivateMessage message)
        {
            message = SanitizeMessage(message);
            _privateMessageRepository.Update(message); 
        }

        /// <summary>
        /// Return list of paged private messages by sent user
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public IPagedList<PrivateMessage> GetPagedSentMessagesByUser(int pageIndex, int pageSize, MembershipUser user)
        {
            return _privateMessageRepository.GetPagedSentMessagesByUser(pageIndex, pageSize, user);
        }

        /// <summary>
        /// Return list of paged private messages by received user
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public IPagedList<PrivateMessage> GetPagedReceivedMessagesByUser(int pageIndex, int pageSize, MembershipUser user)
        {
            return _privateMessageRepository.GetPagedReceivedMessagesByUser(pageIndex, pageSize, user);
        }

        /// <summary>
        /// Gets the last sent private message from a specific user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PrivateMessage GetLastSentPrivateMessage(Guid Id)
        {
            return _privateMessageRepository.GetLastSentPrivateMessage(Id);
        }

        /// <summary>
        /// Gets all private messages sent by a user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllSentByUser(Guid Id)
        {
            return _privateMessageRepository.GetAllSentByUser(Id);
        }

        /// <summary>
        /// Returns a count of any new messages the user has
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int NewPrivateMessageCount(Guid userId)
        {
            return _privateMessageRepository.NewPrivateMessageCount(userId);
        }

        /// <summary>
        /// Gets all private messages received by a user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllReceivedByUser(Guid Id)
        {
            return _privateMessageRepository.GetAllReceivedByUser(Id);
        }


        /// <summary>
        /// get all private messages sent from one user to another
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllByUserToAnotherUser(Guid senderId, Guid receiverId)
        {
            return _privateMessageRepository.GetAllByUserToAnotherUser(senderId, receiverId);
        }

        /// <summary>
        /// Delete a private message
        /// </summary>
        /// <param name="message"></param>
        public void DeleteMessage(PrivateMessage message)
        {
            _privateMessageRepository.Delete(message);
        }

    }
}
