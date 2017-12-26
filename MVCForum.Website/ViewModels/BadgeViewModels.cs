namespace MvcForum.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class VoteBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class MarkAsSolutionBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class FavouriteViewModel
    {
        public Guid FavouriteId { get; set; }
    }

    public class PostBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class TimeBadgeViewModel
    {
        public Guid Id { get; set; }
    }

    public class AllBadgesViewModel
    {
        public IList<Badge> AllBadges { get; set; }
    }
}