namespace MvcForum.Web.ViewModels.Poll
{
    using System;

    public class UpdatePollViewModel
    {
        public Guid PollId { get; set; }
        public Guid AnswerId { get; set; }
    }
}