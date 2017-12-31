namespace MvcForum.Web.ViewModels.Badge
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class AllBadgesViewModel
    {
        public IList<Badge> AllBadges { get; set; }
    }
}