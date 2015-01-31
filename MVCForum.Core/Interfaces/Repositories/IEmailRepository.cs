using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IEmailRepository
    {
        Email Add(Email email);
        void Delete(Email email);
        List<Email> GetAll(int amountToTake);
    }
}
