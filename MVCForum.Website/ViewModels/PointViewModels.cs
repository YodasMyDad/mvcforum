namespace MvcForum.Web.ViewModels
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class HighEarnersPointViewModel
    {
        public Dictionary<MembershipUser, int> HighEarners { get; set; }
    }
}