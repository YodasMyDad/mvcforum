using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class PrivateMessageService : IPrivateMessageService
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
                message = _privateMessageRepository.Add(message);

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

                _privateMessageRepository.Add(sentMessage);

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

        public IPagedList<PrivateMessageListItem> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser user)
        {
            return _privateMessageRepository.GetUsersPrivateMessages(pageIndex, pageSize, user);
        }

        public IPagedList<PrivateMessage> GetUsersPrivateMessages(int pageIndex, int pageSize, MembershipUser toUser, MembershipUser fromUser)
        {
            return _privateMessageRepository.GetUsersPrivateMessages(pageIndex, pageSize, toUser, fromUser);
        }


        /// <summary>
        /// Gets the last sent private message from a specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PrivateMessage GetLastSentPrivateMessage(Guid id)
        {
            return _privateMessageRepository.GetLastSentPrivateMessage(id);
        }

        public PrivateMessage GetMatchingSentPrivateMessage(DateTime date, Guid senderId, Guid receiverId)
        {
            return _privateMessageRepository.GetMatchingSentPrivateMessage(date, senderId, receiverId);
        }

        /// <summary>
        /// Gets all private messages sent by a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllSentByUser(Guid id)
        {
            return _privateMessageRepository.GetAllSentByUser(id);
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
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<PrivateMessage> GetAllReceivedByUser(Guid id)
        {
            return _privateMessageRepository.GetAllReceivedByUser(id);
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
