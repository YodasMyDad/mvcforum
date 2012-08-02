using System;

namespace MVCForum.Website.ViewModels
{
    public class VoteUpBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class MarkAsSolutionBadgeViewModel
    {
        public Guid PostId { get; set; }
    }

    public class TimeBadgeViewModel
    {
        public Guid Id { get; set; }
    }
}