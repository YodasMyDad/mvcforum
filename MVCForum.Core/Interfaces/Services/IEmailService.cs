namespace MvcForum.Core.Interfaces.Services
{
    using System.Collections.Generic;
    using Models;
    using Models.Entities;

    public partial interface IEmailService
    {
        void SendMail(Email email, Settings settings);
        void SendMail(Email email);
        void SendMail(List<Email> email);
        void SendMail(List<Email> email, Settings settings);
        void ProcessMail(List<Email> emails);
        string EmailTemplate(string to, string content);
        string EmailTemplate(string to, string content, Settings settings);
    }
}