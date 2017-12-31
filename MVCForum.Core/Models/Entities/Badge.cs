namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Utilities;

    public partial class Badge : Entity
    {
        /// <summary>
        ///     Specifies the target badge interface names matched to the corresponding badge type
        /// </summary>
        public static readonly Dictionary<BadgeType, string> BadgeClassNames = new Dictionary<BadgeType, string>
        {
            {BadgeType.VoteUp, "MvcForum.Core.Interfaces.Badges.IVoteUpBadge"},
            {BadgeType.MarkAsSolution, "MvcForum.Core.Interfaces.Badges.IMarkAsSolutionBadge"},
            {BadgeType.Time, "MvcForum.Core.Interfaces.Badges.ITimeBadge"},
            {BadgeType.Post, "MvcForum.Core.Interfaces.Badges.IPostBadge"},
            {BadgeType.VoteDown, "MvcForum.Core.Interfaces.Badges.IVoteDownBadge"},
            {BadgeType.Profile, "MvcForum.Core.Interfaces.Badges.IProfileBadge"},
            {BadgeType.Favourite, "MvcForum.Core.Interfaces.Badges.IFavouriteBadge"},
            {BadgeType.Tag, "MvcForum.Core.Interfaces.Badges.ITagBadge"}
        };

        public Badge()
        {
            Id = GuidComb.GenerateComb();
        }

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