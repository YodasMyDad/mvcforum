using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{

    public partial class MembershipFirm : Entity
    {
        public MembershipFirm()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string FirmName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public string SizeBanding { get; set; }
        public bool US { get; set; }
        public bool Canada{ get; set; }
        public bool UK { get; set; }
        public bool EMEA { get; set; }
        public bool APAC { get; set; }
        public bool Other { get; set; }
        public bool ProfessionalServices { get; set; }
        public bool Vendor { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime? MemberInfoCheck { get; set; }
        public DateTime? SizeCheck { get; set; }
        public string Slug { get; set; }
        public string Comment { get; set; }
        public string Website { get; set; }
        public virtual IList<MembershipUser> User { get; set; }
    }
}