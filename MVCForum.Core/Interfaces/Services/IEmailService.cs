using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IEmailService
    {
        void SendMail(Email email);
        void SendMail(List<Email> email);
        void ProcessMail(int amountToSend);
        string EmailTemplate(string to, string content);
    }
}
