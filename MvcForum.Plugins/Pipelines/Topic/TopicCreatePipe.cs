namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class TopicCreatePipe : IPipe<IPipelineProcess<Topic>>
    {
        /// <inheritdoc />
        public Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            throw new NotImplementedException();
        }
    }
}