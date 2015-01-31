using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Email : Entity
    {
        public Email()
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.Now;
        }
        public Guid Id { get; set; }
        public string EmailTo { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public string NameTo { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
