namespace MvcForum.Web.ViewModels.Badge
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class AllBadgesViewModel
    {
        public IList<Badge> AllBadges { get; set; }
    }
}