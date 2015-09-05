using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class BannedEmail
    {
        public BannedEmail()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
