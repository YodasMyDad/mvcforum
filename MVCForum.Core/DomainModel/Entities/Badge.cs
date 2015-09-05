using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public enum BadgeType
    {
        VoteUp,
        VoteDown,
        MarkAsSolution,
        Time,
        Post,
        Profile,
        Favourite,
        Tag
    }

    public partial class Badge : Entity
    {
        public Badge()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// Specifies the target badge interface names matched to the corresponding badge type
        /// </summary>
        public static readonly Dictionary<BadgeType, string> BadgeClassNames = new Dictionary<BadgeType, string>
                                                            {
                                                                {BadgeType.VoteUp, "MVCForum.Domain.Interfaces.Badges.IVoteUpBadge"},
                                                                {BadgeType.MarkAsSolution, "MVCForum.Domain.Interfaces.Badges.IMarkAsSolutionBadge"},
                                                                {BadgeType.Time, "MVCForum.Domain.Interfaces.Badges.ITimeBadge"},
                                                                {BadgeType.Post, "MVCForum.Domain.Interfaces.Badges.IPostBadge"},
                                                                {BadgeType.VoteDown, "MVCForum.Domain.Interfaces.Badges.IVoteDownBadge"},
                                                                {BadgeType.Profile, "MVCForum.Domain.Interfaces.Badges.IProfileBadge"},
                                                                {BadgeType.Favourite, "MVCForum.Domain.Interfaces.Badges.IFavouriteBadge"},
                                                                {BadgeType.Tag, "MVCForum.Domain.Interfaces.Badges.ITagBadge"}
                                                            };

        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Milestone { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? AwardsPoints { get; set; }
        public virtual IList<MembershipUser> Users { get; set; }

    }
}
