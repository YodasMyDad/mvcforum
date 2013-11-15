using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCForum.Domain.DomainModel
{
    public class Report
    {
        public string Reason { get; set; }
        public virtual MembershipUser Reporter { get; set; }
        public virtual MembershipUser ReportedMember { get; set; }
        public virtual Post ReportedPost { get; set; }
    }
}
