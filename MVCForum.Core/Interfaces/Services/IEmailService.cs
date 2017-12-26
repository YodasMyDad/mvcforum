namespace MvcForum.Core.Interfaces.Services
{
    using System.Collections.Generic;
    using DomainModel.Entities;

    public partial interface IEmailService
    {
        void SendMail(Email email, Settings settings);
        void SendMail(Email email);
        void SendMail(List<Email> email);
        void SendMail(List<Email> email, Settings settings);
        void ProcessMail(int amountToSend);
        string EmailTemplate(string to, string content);
        string EmailTemplate(string to, string content, Settings settings);
        Email Add(Email email);
        void Delete(Email email);
        List<Email> GetAll(int amountToTake);
    }
}