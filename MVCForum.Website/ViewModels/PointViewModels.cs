using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class HighEarnersPointViewModel
    {
        public Dictionary<MembershipUser, int> HighEarners { get; set; }
    }
}