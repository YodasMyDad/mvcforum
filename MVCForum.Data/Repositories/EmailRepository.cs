using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class EmailRepository : IEmailRepository
    {
        private readonly MVCForumContext _context;
        public EmailRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Email Add(Email email)
        {
            return _context.Email.Add(email);
        }

        public void Delete(Email email)
        {
            _context.Email.Remove(email);
        }

        public List<Email> GetAll(int amountToTake)
        {
            return _context.Email.OrderBy(x => x.DateCreated).Take(amountToTake).ToList();
        }
    }
}
