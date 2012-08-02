using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IEmailService
    {
        void SendMail(Email email);
        void SendMail(List<Email> email);
        string EmailTemplate(string to, string content);
    }
}
