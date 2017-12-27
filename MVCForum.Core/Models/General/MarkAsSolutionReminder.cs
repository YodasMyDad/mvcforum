namespace MvcForum.Core.DomainModel.General
{
    using Entities;

    public class MarkAsSolutionReminder
    {
        public Topic Topic { get; set; }
        public int PostCount { get; set; }
    }
}
