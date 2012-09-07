using System;

namespace MVCForum.Website.ViewModels
{
    public class VoteUpViewModel
    {
        public Guid Post { get; set; }
    }

    public class VoteDownViewModel
    {
        public Guid Post { get; set; }
    }

    public class MarkAsSolutionViewModel
    {
        public Guid Post { get; set; }
    }

}