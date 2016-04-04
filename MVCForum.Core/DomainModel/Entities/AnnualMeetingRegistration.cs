using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class AnnualMeetingRegistration : Entity
    {
        public AnnualMeetingRegistration()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual MembershipUser MembershipUser { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime EventDate { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string EMail { get; set; }
        public string FirmName { get; set; }
    }
}
