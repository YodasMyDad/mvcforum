namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class TopicNotificationsAndBadgesPipe : IPipe<IPipelineProcess<Topic>>
    {
        /// <inheritdoc />
        public Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // If tags
            //_badgeService.ProcessBadge(BadgeType.Tag, topic.User);

            throw new NotImplementedException();
        }
    }
}