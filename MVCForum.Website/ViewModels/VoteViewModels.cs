namespace MvcForum.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

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

    public class GetVotersViewModel
    {
        public Guid Post { get; set; }
    }

    public class ShowVotersViewModel
    {
        public List<Vote> Votes { get; set; }
    }

    public class ShowVotesViewModel
    {
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
    }
}