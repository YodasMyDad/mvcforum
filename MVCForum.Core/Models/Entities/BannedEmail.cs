namespace MvcForum.Core.Models.Entities
{
    using System;
    using Utilities;

    public partial class BannedEmail : Entity
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
